using System;
using System.Collections.Generic;
using PCG.BiomeData;
using UnityEngine;
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
        [SerializeField] private int worldGridSize = 32;
        private const int c_chunkSize = 16; // m, Unity unit
        private const int c_foliageSubGridSize = c_chunkSize;
        private const int c_foliageSubCellSize = 1; // m, Unity unit

        private Grid grid;

        private Transform environmentParent;
        private Transform floorsParent;

        [SerializeField] private GameObject floorPrefab;

        [SerializeField] private WhittakerBiomes whittakerBiomes = new WhittakerBiomes();

        [SerializeField] private GameObject aimPlanePrefab;

        [SerializeField] private GameObject ocean;

        [Header("Biomes")]
        public Biome[] biomes;

        private void Awake()
        {
            // Get Grid component
            grid = GetComponent<Grid>();
            // Change grid's cell size accordingly
            grid.cellSize = new Vector3(c_chunkSize, 1, c_chunkSize);

            // Initialize biomes parameters
            whittakerBiomes.initializeParameters(worldGridSize);

            // Adjust floor cell scale according to grid cell size, because prefab is plane
            floorPrefab.transform.localScale = new Vector3(c_chunkSize / 10f, 1, c_chunkSize / 10f);
            // Offset self to align with world center
            transform.position = new Vector3(-worldGridSize * c_chunkSize / 2f, 0, -worldGridSize * c_chunkSize / 2f);
            // Generate aim plane. Same size as the terrain.
            GameObject aimPlane = Instantiate(aimPlanePrefab);
            aimPlane.transform.localScale = floorPrefab.transform.localScale * worldGridSize;

            // Make sure biomes array is correct in size and types
            Array biomeTypeValues = Enum.GetValues(typeof(BiomeType));
            Assert.AreEqual(biomeTypeValues.Length, biomes.Length);
            for (int i = 0; i < biomes.Length; i++) {
                Assert.AreEqual(biomes[i].biomeType, biomeTypeValues.GetValue(i));
            }

            // Resize the ocean
            ResizeOcean();

            // Generate the world
            Generate();
        }

        private void ResizeOcean()
        {
            ocean.transform.localScale = new Vector3(worldGridSize * c_chunkSize / 10f, 1, worldGridSize * c_chunkSize / 10f);
            // Adjust ocean's ripple density according to world size
            // TODO: Kind of hacky
            float rippleDensity = ocean.GetComponent<MeshRenderer>().material.GetFloat("_RippleDensity");
            ocean.GetComponent<MeshRenderer>().material.SetFloat("_RippleDensity", rippleDensity * worldGridSize);
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

            // Clear biomes cells
            foreach (Biome biome in biomes) {
                biome.chunks.Clear();
            }

            /* Generate the game world */
            // Get a new random value for biomes generation perlin noises
            whittakerBiomes.RandomizePerlinNoisesOffsets();

            // Generate the world map
            GenerateFloors();
            GeneratePlants();
        }

        private void GenerateFloors()
        {
            for (int x = 0; x < worldGridSize; x++) {
                for (int y = 0; y < worldGridSize; y++) {
                    Vector3 cellCoord = grid.GetCellCenterWorld(new Vector3Int(x, 0, y));
                    BiomeType biomeType = whittakerBiomes.DetermineBiome(x, y);

                    // Spawn a floor tile if it's not Ocean
                    if (biomeType != BiomeType.Ocean) {
                        GameObject floor = Instantiate(floorPrefab, new Vector3(cellCoord.x, 0, cellCoord.z), Quaternion.identity, floorsParent);
                        floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;
                    }

                    // Add this tile to biomes lookup table
                    biomes[biomeType.GetHashCode()].chunks.Add(new Chunk(biomeType, new Vector2Int(x, y)));
                }
            }
        }

        private void GeneratePlants()
        {
            foreach (Biome biome in biomes) {
                foreach (Chunk chunk in biome.chunks) {
                    // (Precalculate) tile bottom left corner world coordinate
                    Vector3 cellBottomLeftCoord = grid.GetCellCenterWorld(new Vector3Int(chunk.cellCoord.x, 0, chunk.cellCoord.y)) - new Vector3(c_chunkSize / 2f, 0, c_chunkSize / 2f);
                    cellBottomLeftCoord.y = 0;

                    // Prepare index list
                    List<int> choices = new List<int>();
                    for (int i = 0; i < c_foliageSubGridSize * c_foliageSubGridSize; i++) {
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
                            int x = subCellIndex % c_foliageSubGridSize;
                            int y = subCellIndex / c_foliageSubGridSize;

                            // Calculate sub cell bottom left corner world coordinate
                            Vector3 subCellBottomLeftCoord = cellBottomLeftCoord + new Vector3(x * c_foliageSubCellSize, 0, y * c_foliageSubCellSize);

                            // Choose a random location within this sub cell, a random scale, and place the plant
                            Vector3 offset = new Vector3(Random.Range(0f, c_foliageSubCellSize), 0, Random.Range(0f, c_foliageSubCellSize));
                            Vector3 scale = Vector3.one * Random.Range(foliageConfig.minSize, foliageConfig.maxSize);
                            GenerateRandomGameObjectFromList(foliageConfig.prefabs, subCellBottomLeftCoord + offset, scale, environmentParent);
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
            go.transform.localScale = scale;
        }
    }
}
