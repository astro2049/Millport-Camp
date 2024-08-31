using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Abilities.ClearingDistance;
using Entities.Ocean;
using Gameplay;
using Gameplay.Quests;
using Managers;
using Managers.GameManager;
using PCG.BiomeData;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace PCG
{
    [Serializable]
    public class Biome
    {
        public BiomeType biomeType;
        public readonly List<Chunk> chunks = new List<Chunk>();
        public BiomeData.BiomeData biomeData;
    }

    public class LevelGenerator : MonoBehaviour
    {
        private const int c_chunkSubGridSize = WorldConfigurations.c_chunkSize;
        private const int c_chunkSubCellSize = 1; // m, Unity unit

        [SerializeField] private GridComponent gridComponent;
        [SerializeField] private MinimapGenerator minimapGenerator;
        [SerializeField] private NavMeshSurface navMeshSurface;

        [Header("Environment")]
        [SerializeField] private Transform levelParent;
        [SerializeField] private Transform floorsParent;
        [SerializeField] private Transform basesParent;
        [SerializeField] private Transform foliageParent;
        [SerializeField] private Transform combatRobotsParent;
        [SerializeField] private Transform vehiclesParent;
        [SerializeField] private Transform zombiesParent;
        [SerializeField] private Transform invisibleWallsParent;
        [SerializeField] private OceanComponent ocean;

        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject oceanWallCubePrefab;

        [SerializeField] private WhittakerBiomes whittakerBiomes = new WhittakerBiomes();

        [Header("Gameplay")]
        [SerializeField] private GameObject aimPlane;
        [SerializeField] private GameplayData gameplayData;

        // Quests
        [SerializeField] private QuestManager questManager;

        [Header("Biomes")]
        public Biome[] biomes;

        // Placement
        private readonly HashSet<Chunk> humanActivityChunks = new HashSet<Chunk>();

        // TODO: hacky order...
        private void Initialize()
        {
            // Get components
            minimapGenerator = GetComponent<MinimapGenerator>();
            gridComponent.Initialize();

            // Initialize biomes parameters
            whittakerBiomes.initializeParameters(WorldConfigurations.s_worldGridSize);

            // Adjust floor cell scale according to grid cell size, because prefab is plane
            floorPrefab.transform.localScale = new Vector3(WorldConfigurations.c_chunkSize / 10f, 1, WorldConfigurations.c_chunkSize / 10f);
            // Offset level parent to align with world center
            levelParent.transform.position = new Vector3(-WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 2f, 0, -WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 2f);
            // Generate aim plane. Same size as the terrain.
            aimPlane.transform.localScale = floorPrefab.transform.localScale * WorldConfigurations.s_worldGridSize;

            // Make sure biomes array is correct in size and types
            Array biomeTypeValues = Enum.GetValues(typeof(BiomeType));
            Assert.AreEqual(biomeTypeValues.Length, biomes.Length);
            for (int i = 0; i < biomes.Length; i++) {
                Assert.AreEqual(biomes[i].biomeType, biomeTypeValues.GetValue(i));
            }

            // Resize ocean invisible wall cube
            oceanWallCubePrefab.transform.localScale = Vector3.one * WorldConfigurations.c_chunkSize;
        }

        /* Generate the game world */
        public void Generate()
        {
            Initialize();

            // Resize the ocean
            ocean.Resize(WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize);

            // Generate the world map
            GenerateFloors();
            PlaceBases();
            PlacePlants();

            // Wait for the current frame to finish. This is because there are Destroy() calls in Generate()
            StartCoroutine(LevelGenerationPart2());
        }

        private IEnumerator LevelGenerationPart2()
        {
            yield return new WaitForEndOfFrame();

            // Build nav mesh
            navMeshSurface.BuildNavMesh();

            // Take a bird's eye view shot of the map (for in-game minimap and outputting png)
            minimapGenerator.StreamWorldToMinimap(WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize);

            PlaceCombatRobots();
            PlaceVehicles();
            PlacePlayer();
            PlaceZombies();

            // Start the first quest
            if (questManager.quests.Length > 0) {
                questManager.StartStory();
            }
        }

        private void GenerateFloors()
        {
            for (int x = 0; x < WorldConfigurations.s_worldGridSize; x++) {
                for (int y = 0; y < WorldConfigurations.s_worldGridSize; y++) {
                    // Determine chunk's biome type
                    BiomeType biomeType = whittakerBiomes.DetermineBiome(x, y);

                    // Add this chunk to biomes lookup table
                    Chunk chunk = new Chunk(biomeType, new Vector2Int(x, y));
                    biomes[biomeType.GetHashCode()].chunks.Add(chunk);

                    // Spawn a floor tile if it's not Ocean
                    if (biomeType == BiomeType.Ocean) {
                        Instantiate(oceanWallCubePrefab, gridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, invisibleWallsParent);
                        continue;
                    }
                    GameObject floor = Instantiate(floorPrefab, gridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, floorsParent);
                    floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;
                }
            }
        }

        private void PlaceBases()
        {
            int i = 0;
            foreach (Quest quest in questManager.quests) {
                List<Chunk> chunks = biomes[quest.baseBiome.GetHashCode()].chunks;
                Chunk chunk = chunks[chunks.Count / 2];
                humanActivityChunks.Add(chunk);

                // Get chunk center world coordinate
                Vector3 chunkCenter = gridComponent.GetChunkCenterWorld(chunk);

                // Place base in the center
                GameObject researchBase = Instantiate(quest.basePrefab, chunkCenter, Quaternion.identity, basesParent);
                if (i == 1) {
                    researchBase.transform.rotation = Quaternion.Euler(0, 120, 0);
                } else if (i == 2) {
                    researchBase.transform.Translate(new Vector3(-2.5f, 0, -2.5f));
                    researchBase.transform.rotation = Quaternion.Euler(0, 180, 0);
                }

                // Assign this research base to the corresponding quest
                quest.AssignDestinationGo(researchBase);

                i++;
            }
        }

        private void PlacePlants()
        {
            foreach (Biome biome in biomes) {
                if (biome.biomeType == BiomeType.Ocean || biome.biomeType == BiomeType.Mountain) {
                    continue;
                }

                foreach (Chunk chunk in biome.chunks) {
                    if (humanActivityChunks.Contains(chunk)) {
                        continue;
                    }

                    // Precalculate chunk bottom left world coordinate
                    Vector3 chunkBottomLeft = gridComponent.GetChunkBottomLeftCornerWorld(chunk);

                    // Prepare foliage sub cell choices index list
                    List<int> choices = new List<int>();
                    for (int i = 0; i < c_chunkSubGridSize * c_chunkSubGridSize; i++) {
                        choices.Add(i);
                    }
                    foreach (FoliageConfig foliageConfig in biome.biomeData.foliageConfigs) {
                        // Determine if to generate the plant set at all
                        if (Random.Range(0f, 1f) >= foliageConfig.occurence) {
                            continue;
                        }

                        // Choose the sub-cells to generate this plant randomly
                        List<int> subCellIndices = GetXElementsFromYElements(foliageConfig.subCellCount, choices);

                        // Place the plants on the selected sub cells
                        foreach (int subCellIndex in subCellIndices) {
                            // Calculate sub cell x, y coordinate (in sub grid)
                            int x = subCellIndex % c_chunkSubGridSize;
                            int y = subCellIndex / c_chunkSubGridSize;

                            // Calculate sub cell bottom left corner world coordinate
                            Vector3 subCellBottomLeftCoordinate = chunkBottomLeft + new Vector3(x * c_chunkSubCellSize, 0, y * c_chunkSubCellSize);

                            // Choose a random location within this sub cell, a random scale, and place the plant
                            Vector3 offset = new Vector3(Random.Range(0f, c_chunkSubCellSize), 0, Random.Range(0f, c_chunkSubCellSize));
                            Vector3 scale = Vector3.one * Random.Range(foliageConfig.minSize, foliageConfig.maxSize);
                            GenerateRandomGameObjectFromList(foliageConfig.prefabs, subCellBottomLeftCoordinate + offset, scale, foliageParent);
                        }
                    }
                }
            }
        }

        private List<int> GetXElementsFromYElements(int x, List<int> choices)
        {
            List<int> chosenOnes = new List<int>();
            for (int i = 0; i < x; i++) {
                // Choose an index (for the element) randomly
                int index = Random.Range(0, choices.Count - i);
                // Add this element to the return list
                chosenOnes.Add(choices[index]);

                // In the choices list, replace this element with the last element
                choices[index] = choices[choices.Count - 1];
                // And remove the last element
                choices.RemoveAt(choices.Count - 1);
            }

            return chosenOnes;
        }

        private void GenerateRandomGameObjectFromList(GameObject[] options, Vector3 coord, Vector3 scale, Transform parent)
        {
            GameObject go = Instantiate(options[Random.Range(0, options.Length)], coord, Quaternion.identity, parent);
            // Apply random scale
            go.transform.localScale = scale;
            // Assign layer "Structure"
            go.layer = LayerMask.NameToLayer("Structure");
            // Add component MaterialShifter
            go.AddComponent<MaterialShifterComponent>();
        }

        private readonly float navmeshPlacementSampleDistance = 1f;

        [Header("Combat Robots")]
        [SerializeField] private GameObject combatRobotPrefab;
        [SerializeField] private int combatRobotSectorMinCount = 8;
        [SerializeField] private int combatRobotSectorMaxCount = 15;

        private void PlaceCombatRobots()
        {
            foreach (Quest quest in questManager.quests) {
                Vector3 sampleCenter = quest.destinationGo.transform.position;
                int combatRobotCount = Random.Range(combatRobotSectorMinCount, combatRobotSectorMaxCount);

                for (int i = 0; i < combatRobotCount; i++) {
                    Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(7f, WorldConfigurations.c_chunkSize / 2f + 5);
                    NavMesh.SamplePosition(sampleCenter + new Vector3(offset.x, 0, offset.y), out NavMeshHit hit, navmeshPlacementSampleDistance, NavMesh.AllAreas);
                    if (hit.hit) {
                        GameObject combatRobot = Instantiate(combatRobotPrefab, hit.position, Quaternion.identity, combatRobotsParent);
                        combatRobot.transform.Rotate(Vector3.up, Random.Range(0, 180));
                    }
                }
            }
        }

        [Header("Vehicles")]
        [SerializeField] private GameObject vehiclePrefab;
        [SerializeField] private Material[] vehicleMaterials;
        [SerializeField] [Range(0f, 1f)] private float vehicleChunkOccurence = 0.1f;

        private void PlaceVehicles()
        {
            foreach (Biome biome in biomes) {
                if (biome.biomeType == BiomeType.Ocean || biome.biomeType == BiomeType.Mountain) {
                    continue;
                }

                foreach (Chunk chunk in biome.chunks) {
                    if (humanActivityChunks.Contains(chunk)) {
                        continue;
                    }
                    if (Random.Range(0f, 1f) >= vehicleChunkOccurence) {
                        continue;
                    }

                    Vector3 chunkCenter = gridComponent.GetChunkCenterWorld(chunk);
                    Vector2 offset = Random.insideUnitCircle * Random.Range(0f, WorldConfigurations.c_chunkSize / 4f);
                    NavMesh.SamplePosition(chunkCenter + new Vector3(offset.x, 0, offset.y) + new Vector3(offset.x, 0, offset.y), out NavMeshHit hit, navmeshPlacementSampleDistance, NavMesh.AllAreas);
                    if (hit.hit) {
                        GameObject vehicle = Instantiate(vehiclePrefab, hit.position, Quaternion.identity, vehiclesParent);
                        vehicle.transform.Rotate(Vector3.up, Random.Range(0, 180));
                        vehicle.GetComponent<MeshRenderer>().material = vehicleMaterials[Random.Range(0, vehicleMaterials.Length)];
                    }
                }
            }
        }

        private bool PlacePlayer()
        {
            // Place player in the woodland biome, in the bottom right corner
            Biome woodlandBiome = biomes[BiomeType.Woodland.GetHashCode()];

            int spawnChunkIndex = woodlandBiome.chunks.Count / 3 * 2;
            // Not to place on the base chunk
            if (spawnChunkIndex == woodlandBiome.chunks.Count / 2) {
                spawnChunkIndex--;
            }

            Chunk chunk = woodlandBiome.chunks[spawnChunkIndex];
            humanActivityChunks.Add(chunk);
            Vector3 chunkCenter = gridComponent.GetChunkCenterWorld(chunk);
            NavMesh.SamplePosition(chunkCenter, out NavMeshHit hit, 20f, NavMesh.AllAreas);
            if (hit.hit) {
                gameplayData.player.transform.position = hit.position;
                return true;
            }
            return false;
        }

        // Zombies
        [Header("Zombies")]
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] [Range(0f, 1f)] private float zombieChunkOccurence;
        [SerializeField] private int zombieMinCountPerChunk;
        [SerializeField] private int zombieMaxCountPerChunk;

        private void PlaceZombies()
        {
            foreach (Biome biome in biomes) {
                if (biome.biomeType == BiomeType.Ocean || biome.biomeType == BiomeType.Mountain) {
                    continue;
                }

                foreach (Chunk chunk in biome.chunks) {
                    if (humanActivityChunks.Contains(chunk)) {
                        continue;
                    }
                    if (Random.Range(0f, 1f) >= zombieChunkOccurence) {
                        continue;
                    }

                    /* Place zombies */
                    // Determine zombie count on this chunk
                    int zombieCount = Random.Range(zombieMinCountPerChunk, zombieMaxCountPerChunk);
                    // Precalculate chunk bottom left world coordinate
                    Vector3 chunkBottomLeft = gridComponent.GetChunkBottomLeftCornerWorld(chunk);

                    // TODO: Duplication in PlacePlants()
                    // Prepare sub cell choices index list
                    List<int> subCellIndexChoices = new List<int>();
                    for (int i = 0; i < c_chunkSubGridSize * c_chunkSubGridSize; i++) {
                        subCellIndexChoices.Add(i);
                    }
                    // Choose the sub-cells to generate zombies randomly
                    List<int> subCellIndices = GetXElementsFromYElements(zombieCount, subCellIndexChoices);

                    // Place the zombies on the selected sub cells
                    foreach (int subCellIndex in subCellIndices) {
                        // Calculate sub cell x, y coordinate (in sub grid)
                        int x = subCellIndex % c_chunkSubGridSize;
                        int y = subCellIndex / c_chunkSubGridSize;

                        // Calculate sub cell bottom left corner world coordinate
                        Vector3 subCellBottomLeft = chunkBottomLeft + new Vector3(x * c_chunkSubCellSize, 0, y * c_chunkSubCellSize);
                        // Choose a random location within this sub cell, sample closest point on navmesh, and place the zombie
                        Vector3 offset = new Vector3(Random.Range(0f, c_chunkSubCellSize), 0, Random.Range(0f, c_chunkSubCellSize));
                        Vector3 sampleLocation = subCellBottomLeft + offset;

                        NavMesh.SamplePosition(sampleLocation, out NavMeshHit hit, navmeshPlacementSampleDistance, NavMesh.AllAreas);
                        if (hit.hit) {
                            GameObject zombie = Instantiate(zombiePrefab, hit.position, Quaternion.identity, zombiesParent);
                            zombie.transform.Rotate(Vector3.up, Random.Range(0, 180));
                        }
                    }
                }
            }
        }
    }
}
