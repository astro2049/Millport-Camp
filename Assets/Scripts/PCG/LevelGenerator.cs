using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Abilities.ClearingDistance;
using Gameplay;
using Gameplay.Quests;
using Managers;
using PCG.BiomeData;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.Events;
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

        private GridComponent gridComponent;
        private MinimapGenerator minimapGenerator;
        private NavMeshSurface navMeshSurface;

        private Transform environmentParent;
        private Transform floorsParent;
        private Transform foliageParent;
        private Transform basesParent;
        private Transform zombiesParent;
        private Transform combatRobotsParent;
        private Transform vehiclesParent;

        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject oceanWallCubePrefab;

        [SerializeField] private WhittakerBiomes whittakerBiomes = new WhittakerBiomes();

        [Header("Gameplay")]
        [SerializeField] private GameObject aimPlanePrefab;
        [SerializeField] private GameObject ocean;

        // Quests
        [SerializeField] private QuestManager questManager;

        [Header("Biomes")]
        public Biome[] biomes;

        // Placement
        private readonly HashSet<Chunk> humanActivityChunks = new HashSet<Chunk>();

        [HideInInspector] public UnityEvent<GameObject> levelGenerated = new UnityEvent<GameObject>();

        private void Awake()
        {
            // Get components
            gridComponent = GetComponent<GridComponent>();
            minimapGenerator = GetComponent<MinimapGenerator>();
            navMeshSurface = GetComponent<NavMeshSurface>();

            // Initialize biomes parameters
            whittakerBiomes.initializeParameters(WorldConfigurations.s_worldGridSize);

            // Adjust floor cell scale according to grid cell size, because prefab is plane
            floorPrefab.transform.localScale = new Vector3(WorldConfigurations.c_chunkSize / 10f, 1, WorldConfigurations.c_chunkSize / 10f);
            // Offset self to align with world center
            transform.position = new Vector3(-WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 2f, 0, -WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 2f);
            // Generate aim plane. Same size as the terrain.
            GameObject aimPlane = Instantiate(aimPlanePrefab);
            aimPlane.transform.localScale = floorPrefab.transform.localScale * WorldConfigurations.s_worldGridSize;

            // Make sure biomes array is correct in size and types
            Array biomeTypeValues = Enum.GetValues(typeof(BiomeType));
            Assert.AreEqual(biomeTypeValues.Length, biomes.Length);
            for (int i = 0; i < biomes.Length; i++) {
                Assert.AreEqual(biomes[i].biomeType, biomeTypeValues.GetValue(i));
            }

            // Resize the ocean
            ResizeOcean();
            // Resize ocean invisible wall cube
            oceanWallCubePrefab.transform.localScale = Vector3.one * WorldConfigurations.c_chunkSize;
        }

        private void ResizeOcean()
        {
            ocean.transform.localScale = new Vector3(WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 10f, 1, WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize / 10f);
            // Adjust ocean's ripple density according to world size
            // TODO: Kind of hacky
            float rippleDensity = ocean.GetComponent<MeshRenderer>().material.GetFloat("_RippleDensity");
            ocean.GetComponent<MeshRenderer>().material.SetFloat("_RippleDensity", rippleDensity * WorldConfigurations.s_worldGridSize);
        }

        public void Generate()
        {
            // Destroy previous world map
            if (environmentParent) {
                Destroy(environmentParent.gameObject);
            }
            environmentParent = new GameObject("Environment").transform;
            environmentParent.parent = transform;
            floorsParent = new GameObject("Floors").transform;
            floorsParent.parent = environmentParent;
            foliageParent = new GameObject("Foliage").transform;
            foliageParent.parent = environmentParent;
            basesParent = new GameObject("Bases").transform;
            basesParent.parent = environmentParent;
            zombiesParent = new GameObject("Zombies").transform;
            zombiesParent.parent = transform;
            combatRobotsParent = new GameObject("Combat Robots").transform;
            combatRobotsParent.parent = transform;
            vehiclesParent = new GameObject("Vehicles").transform;
            vehiclesParent.parent = transform;

            // Clear biomes cells
            foreach (Biome biome in biomes) {
                biome.chunks.Clear();
            }

            /* Generate the game world */
            // Get a new random value for biomes generation perlin noises
            whittakerBiomes.RandomizePerlinNoisesOffsets();

            // Generate the world map
            GenerateFloors();
            PlacePlants();
            PlaceBases();

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
            GameObject player = PlacePlayer();
            PlaceZombies();

            levelGenerated.Invoke(player);

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
                        Instantiate(oceanWallCubePrefab, gridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, environmentParent);
                        continue;
                    }
                    GameObject floor = Instantiate(floorPrefab, gridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, floorsParent);
                    floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;
                }
            }
        }

        private void PlacePlants()
        {
            foreach (Biome biome in biomes) {
                foreach (Chunk chunk in biome.chunks) {
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

        private void PlaceBases()
        {
            foreach (Quest quest in questManager.quests) {
                List<Chunk> chunks = biomes[quest.baseBiome.GetHashCode()].chunks;
                Chunk chunk = chunks[chunks.Count / 2];
                humanActivityChunks.Add(chunk);

                // Get chunk center world coordinate
                Vector3 chunkCenter = gridComponent.GetChunkCenterWorld(chunk);

                // Clear all foliage in chunk
                Collider[] hitColliders = Physics.OverlapBox(chunkCenter, Vector3.one * (WorldConfigurations.c_chunkSize / 2f), Quaternion.identity, LayerMask.GetMask("Structure"));
                foreach (Collider hitCollider in hitColliders) {
                    Destroy(hitCollider.gameObject);
                }

                // Place base in the center
                GameObject researchBase = Instantiate(quest.basePrefab, chunkCenter, Quaternion.identity, basesParent);
                // Assign this research base to the corresponding quest
                quest.AssignDestinationGo(researchBase);
            }
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

        [Header("Player")]
        [SerializeField] private GameObject playerPrefab;

        private GameObject PlacePlayer()
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
                GameObject player = Instantiate(playerPrefab, hit.position, Quaternion.identity, null);
                return player;
            }
            return null;
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
                        }
                    }
                }
            }
        }
    }
}
