using System;
using System.Collections.Generic;
using Entities.Abilities.ClearingDistance;
using Managers.GameManager;
using Managers.Quests;
using PCG.BiomeData;
using PCG.Generators.Terrain;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace PCG.Generators
{
    public class ActorsPlacer : MonoBehaviour
    {
        [SerializeField] private QuestsManager questsManager;

        [Header("Data")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private WorldData worldData;

        [SerializeField] private GridComponent gridComponent;
        private Biome[] biomes;

        [Header("Environment")]
        [SerializeField] private Transform basesParent;
        [SerializeField] private Transform foliageParent;
        [SerializeField] private Transform combatRobotsParent;
        [SerializeField] private Transform vehiclesParent;
        [SerializeField] private Transform zombiesParent;

        private readonly HashSet<Chunk> humanActivityChunks = new HashSet<Chunk>();
        public List<Vector3> basePositions;

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
            foreach (Quest quest in questsManager.quests) {
                List<Chunk> chunks = biomes[quest.baseBiome.GetHashCode()].chunks;
                Chunk chunk = chunks[Random.Range(0, chunks.Count)];
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
                basePositions.Add(researchBase.transform.position);

                // Assign this research base to the corresponding quest
                quest.AssignDestinationGo(researchBase);

                i++;
            }
        }

        public void PlacePlants()
        {
            ForEachValidChunk((biome, chunk) =>
            {
                // Precalculate chunk bottom left world coordinate
                Vector3 chunkBottomLeft = gridComponent.GetChunkBottomLeftCornerWorld(chunk);

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
                        // Choose a random location
                        Vector3 offset = new Vector3(Random.Range(0f, worldData.actorCellSize), 0, Random.Range(0f, worldData.actorCellSize));
                        Vector3 position = subCellBottomLeftCoordinate + offset;

                        // Test: road below?
                        Ray ray = new Ray(position + Vector3.up * 1f, Vector3.down);
                        // Draw the debug ray
                        // Debug.DrawRay(position + Vector3.up * 1f, Vector3.down * 2f, Color.blue, 10f);
                        if (Physics.Raycast(ray, out RaycastHit hitInfo, 2f, LayerMask.GetMask("Road"), QueryTriggerInteraction.Collide)) {
                            continue;
                        }

                        // Choose a random scale
                        Vector3 scale = Vector3.one * Random.Range(foliageConfig.minSize, foliageConfig.maxSize);
                        GenerateRandomGameObjectFromList(foliageConfig.prefabs, position, scale, foliageParent);
                    }
                }
            });
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
            foreach (Quest quest in questsManager.quests) {
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
            ForEachValidChunk((biome, chunk) =>
            {
                if (Random.Range(0f, 1f) >= vehicleChunkOccurence) {
                    return;
                }

                Vector3 chunkCenter = gridComponent.GetChunkCenterWorld(chunk);
                Vector2 offset = Random.insideUnitCircle * Random.Range(0f, worldData.chunkSize / 4f);
                Vector3 samplePosition = chunkCenter + new Vector3(offset.x, 0, offset.y);

                NavMesh.SamplePosition(samplePosition, out NavMeshHit hit, navmeshPlacementSampleDistance, NavMesh.AllAreas);
                if (hit.hit) {
                    GameObject vehicle = Instantiate(vehiclePrefab, hit.position, Quaternion.identity, vehiclesParent);
                    vehicle.transform.Rotate(Vector3.up, Random.Range(0, 180));
                    vehicle.GetComponent<MeshRenderer>().material = vehicleMaterials[Random.Range(0, vehicleMaterials.Length)];
                }
            });
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

        private void PlaceZombies()
        {
            ForEachValidChunk((biome, chunk) =>
            {
                if (Random.Range(0f, 1f) >= zombieChunkOccurence) {
                    return;
                }

                // Determine zombie count on this chunk
                int zombieCount = Random.Range(zombieMinCountPerChunk, zombieMaxCountPerChunk);
                // Precalculate chunk bottom left world coordinate
                Vector3 chunkBottomLeft = gridComponent.GetChunkBottomLeftCornerWorld(chunk);

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
            });
        }

        private void ForEachValidChunk(Action<Biome, Chunk> action)
        {
            foreach (Biome biome in biomes) {
                if (biome.biomeType == BiomeType.Ocean || biome.biomeType == BiomeType.Mountain) {
                    continue;
                }

                foreach (Chunk chunk in biome.chunks) {
                    if (humanActivityChunks.Contains(chunk)) {
                        continue;
                    }

                    action(biome, chunk);
                }
            }
        }

        private List<int> GetXElementsFromYElements(int x, List<int> choices)
        {
            List<int> chosenOnes = new List<int>();
            for (int i = 0; i < x; i++) {
                if (choices.Count == 0) {
                    break;
                }

                // Choose an index (for the element) randomly
                int index = Random.Range(0, choices.Count);
                // Add this element to the return list
                chosenOnes.Add(choices[index]);

                // Remove the chosen element
                choices.RemoveAt(index);
            }

            return chosenOnes;
        }
    }
}
