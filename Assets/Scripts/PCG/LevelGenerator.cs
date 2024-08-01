using System;
using System.Collections.Generic;
using PCG.BiomeData;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace PCG
{
    public class Tile
    {
        public BiomeType biome;
        public Vector2Int cellCoord;

        public Tile(BiomeType biome, Vector2Int cellCoord)
        {
            this.biome = biome;
            this.cellCoord = cellCoord;
        }
    }

    [Serializable]
    public class Biome
    {
        public BiomeType biomeType;
        public readonly List<Tile> tiles = new List<Tile>();
        public BiomeData.BiomeData biomeData;
    }

    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private int worldGridSize = 32;
        [SerializeField] private int worldCellSize = 8; // m, Unity unit
        [SerializeField] private int foliageSubGridSize = 4;
        private float foliageSubCellSize; // m, Unity unit

        private Grid grid;

        private Transform environmentParent;
        private Transform floorsParent;

        [SerializeField] private GameObject floorPrefab;

        [SerializeField] private WhittakerBiomes whittakerBiomes = new WhittakerBiomes();

        [SerializeField] private GameObject aimPlanePrefab;

        [Header("Biomes")]
        public Biome[] biomes;

        private void Awake()
        {
            // Get Grid component
            grid = GetComponent<Grid>();
            // Change grid's cell size accordingly
            grid.cellSize = new Vector3(worldCellSize, 1, worldCellSize);
            // (Precalculate) Foliage sub cell size
            foliageSubCellSize = (float)worldCellSize / foliageSubGridSize;

            // Initialize biomes parameters
            whittakerBiomes.initializeParameters(worldGridSize);

            // Adjust floor cell scale according to grid cell size, because prefab is plane
            floorPrefab.transform.localScale = new Vector3(worldCellSize / 10f, 1, worldCellSize / 10f);
            // Offset self to align with world center
            transform.position = new Vector3(-worldGridSize * worldCellSize / 2f, 0, -worldGridSize * worldCellSize / 2f);
            // Generate aim plane. Same size as the terrain.
            GameObject aimPlane = Instantiate(aimPlanePrefab);
            aimPlane.transform.localScale = floorPrefab.transform.localScale * worldGridSize;

            // Make sure biomes array is correct in size and types
            Array biomeTypeValues = Enum.GetValues(typeof(BiomeType));
            Assert.AreEqual(biomeTypeValues.Length, biomes.Length);
            for (int i = 0; i < biomes.Length; i++) {
                Assert.AreEqual(biomes[i].biomeType, biomeTypeValues.GetValue(i));
            }

            // Generate the world
            Generate();
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
                biome.tiles.Clear();
            }

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
                    GameObject floor = Instantiate(floorPrefab, new Vector3(cellCoord.x, 0, cellCoord.z), Quaternion.identity, floorsParent);
                    BiomeType biomeType = whittakerBiomes.DetermineBiome(x, y);
                    floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;

                    // Add this tile to biomes lookup table
                    biomes[biomeType.GetHashCode()].tiles.Add(new Tile(biomeType, new Vector2Int(x, y)));
                }
            }
        }

        private void GeneratePlants()
        {
            foreach (Biome biome in biomes) {
                foreach (Tile tile in biome.tiles) {
                    // (Precalculate) tile bottom left corner world coordinate
                    Vector3 cellBottomLeftCoord = grid.GetCellCenterWorld(new Vector3Int(tile.cellCoord.x, 0, tile.cellCoord.y)) - new Vector3(worldCellSize / 2f, 0, worldCellSize / 2f);
                    cellBottomLeftCoord.y = 0;

                    // Prepare index list
                    List<int> choices = new List<int>();
                    for (int i = 0; i < foliageSubGridSize * foliageSubGridSize; i++) {
                        choices.Add(i);
                    }

                    foreach (FoliageConfig foliageConfig in biome.biomeData.foliageConfigs) {
                        // Choose the sub-cells to generate this plant randomly
                        List<int> subCellIndices = GetXElementsFromYElements(foliageConfig.subCellCount, choices);

                        // Place the plants on the selected sub cells
                        foreach (int subCellIndex in subCellIndices) {
                            // Calculate sub cell x, y coordinate (in sub grid)
                            int x = subCellIndex % foliageSubGridSize;
                            int y = subCellIndex / foliageSubGridSize;

                            // Calculate sub cell bottom left corner world coordinate
                            Vector3 subCellBottomLeftCoord = cellBottomLeftCoord + new Vector3(x * foliageSubCellSize, 0, y * foliageSubCellSize);

                            // Choose a random location within this sub cell, a random scale, and place the plant
                            Vector3 offset = new Vector3(Random.Range(0f, foliageSubCellSize), 0, Random.Range(0f, foliageSubCellSize));
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
