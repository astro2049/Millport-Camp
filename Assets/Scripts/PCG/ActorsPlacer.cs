using System.Collections.Generic;
using Entities.Abilities.ClearingDistance;
using Gameplay.Quests;
using Managers;
using Managers.GameManager;
using PCG.BiomeData;
using PCG.Chunks;
using UnityEngine;
using UnityEngine.AI;

namespace PCG
{
    public class ActorsPlacer : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;

        [Header("Data")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private WorldData worldData;

        private ChunkGridComponent chunkGridComponent;
        private Biome[] biomes;

        [Header("Environment")]
        [SerializeField] private Transform basesParent;
        [SerializeField] private Transform foliageParent;
        [SerializeField] private Transform combatRobotsParent;
        [SerializeField] private Transform vehiclesParent;
        [SerializeField] private Transform zombiesParent;

        private readonly HashSet<Chunk> humanActivityChunks = new HashSet<Chunk>();

        private readonly float navmeshPlacementSampleDistance = 1f;
        [Header("Combat Robots")]
        [SerializeField] private GameObject combatRobotPrefab;
        [SerializeField] private int combatRobotSectorMinCount = 8;
        [SerializeField] private int combatRobotSectorMaxCount = 15;

        [Header("Vehicles")]
        [SerializeField] private GameObject vehiclePrefab;
        [SerializeField] private Material[] vehicleMaterials;
        [SerializeField] [Range(0f, 1f)] private float vehicleChunkOccurence = 0.05f;

        [Header("Zombies")]
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] [Range(0f, 1f)] private float zombieChunkOccurence = 0.5f;
        [SerializeField] private int zombieMinCountPerChunk = 1;
        [SerializeField] private int zombieMaxCountPerChunk = 5;

        public void Initialize()
        {
            chunkGridComponent = GetComponent<ChunkGridComponent>();
            biomes = GetComponent<LevelGenerator>().biomes;
        }

        public void PlaceActors()
        {
            PlaceCombatRobots();
            PlaceVehicles();
            PlacePlayer();
            PlaceZombies();
        }

        public void PlaceBases()
        {
            int i = 0;
            foreach (Quest quest in questManager.quests) {
                List<Chunk> chunks = biomes[quest.baseBiome.GetHashCode()].chunks;
                Chunk chunk = chunks[chunks.Count / 2];
                humanActivityChunks.Add(chunk);

                // Get chunk center world coordinate
                Vector3 chunkCenter = chunkGridComponent.GetChunkCenterWorld(chunk);

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

        public void PlacePlants()
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
                    Vector3 chunkBottomLeft = chunkGridComponent.GetChunkBottomLeftCornerWorld(chunk);

                    // Prepare foliage sub cell choices index list
                    List<int> choices = new List<int>();
                    for (int i = 0; i < worldData.actorGridSize * worldData.actorGridSize; i++) {
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
                            int x = subCellIndex % worldData.actorGridSize;
                            int y = subCellIndex / worldData.actorGridSize;

                            // Calculate sub cell bottom left corner world coordinate
                            Vector3 subCellBottomLeftCoordinate = chunkBottomLeft + new Vector3(x * worldData.actorCellSize, 0, y * worldData.actorCellSize);

                            // Choose a random location within this sub cell, a random scale, and place the plant
                            Vector3 offset = new Vector3(Random.Range(0f, worldData.actorCellSize), 0, Random.Range(0f, worldData.actorCellSize));
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

        private void PlaceCombatRobots()
        {
            foreach (Quest quest in questManager.quests) {
                Vector3 sampleCenter = quest.destinationGo.transform.position;
                int combatRobotCount = Random.Range(combatRobotSectorMinCount, combatRobotSectorMaxCount);

                for (int i = 0; i < combatRobotCount; i++) {
                    Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(7f, worldData.chunkSize / 2f + 5);
                    NavMesh.SamplePosition(sampleCenter + new Vector3(offset.x, 0, offset.y), out NavMeshHit hit, navmeshPlacementSampleDistance, NavMesh.AllAreas);
                    if (hit.hit) {
                        GameObject combatRobot = Instantiate(combatRobotPrefab, hit.position, Quaternion.identity, combatRobotsParent);
                        combatRobot.transform.Rotate(Vector3.up, Random.Range(0, 180));
                    }
                }
            }
        }

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

                    Vector3 chunkCenter = chunkGridComponent.GetChunkCenterWorld(chunk);
                    Vector2 offset = Random.insideUnitCircle * Random.Range(0f, worldData.chunkSize / 4f);
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
            Vector3 chunkCenter = chunkGridComponent.GetChunkCenterWorld(chunk);
            NavMesh.SamplePosition(chunkCenter, out NavMeshHit hit, 20f, NavMesh.AllAreas);
            if (hit.hit) {
                gameplayData.player.transform.position = hit.position;
                return true;
            }
            return false;
        }

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
                    Vector3 chunkBottomLeft = chunkGridComponent.GetChunkBottomLeftCornerWorld(chunk);

                    // TODO: Duplication in PlacePlants()
                    // Prepare sub cell choices index list
                    List<int> subCellIndexChoices = new List<int>();
                    for (int i = 0; i < worldData.actorGridSize * worldData.actorGridSize; i++) {
                        subCellIndexChoices.Add(i);
                    }
                    // Choose the sub-cells to generate zombies randomly
                    List<int> subCellIndices = GetXElementsFromYElements(zombieCount, subCellIndexChoices);

                    // Place the zombies on the selected sub cells
                    foreach (int subCellIndex in subCellIndices) {
                        // Calculate sub cell x, y coordinate (in sub grid)
                        int x = subCellIndex % worldData.actorGridSize;
                        int y = subCellIndex / worldData.actorGridSize;

                        // Calculate sub cell bottom left corner world coordinate
                        Vector3 subCellBottomLeft = chunkBottomLeft + new Vector3(x * worldData.actorCellSize, 0, y * worldData.actorCellSize);
                        // Choose a random location within this sub cell, sample closest point on navmesh, and place the zombie
                        Vector3 offset = new Vector3(Random.Range(0f, worldData.actorCellSize), 0, Random.Range(0f, worldData.actorCellSize));
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
