using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace PCG.Generators.Terrain
{
    public enum BiomeType
    {
        Ocean = 0,
        Ice = 1,
        Tundra = 2,
        Taiga = 3,
        Grassland = 4,
        Woodland = 5,
        Desert = 6,
        Mountain = 7
    }

    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private WorldData worldData;
        [SerializeField] private GridComponent gridComponent;
        [SerializeField] private MapFeatures mapFeatures;

        private Biome[] biomes;

        [SerializeField] private Transform floorsParent;
        [SerializeField] private Transform invisibleWallsParent;

        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject oceanWallCubePrefab;

        public void Initialize()
        {
            mapFeatures = GetComponent<MapFeatures>();
            mapFeatures.Initialize();
            biomes = GetComponent<LevelGenerator>().biomes;

            // Adjust floor cell scale according to grid cell size, because prefab is plane
            floorPrefab.transform.localScale = new Vector3(worldData.floorTileSize / 10f, 1, worldData.floorTileSize / 10f);

            // Make sure biomes array is correct in size and types
            Array biomeTypeValues = Enum.GetValues(typeof(BiomeType));
            Assert.AreEqual(biomeTypeValues.Length, biomes.Length);
            for (int i = 0; i < biomes.Length; i++) {
                Assert.AreEqual(biomes[i].biomeType, biomeTypeValues.GetValue(i));
            }

            // Resize ocean invisible wall cube
            oceanWallCubePrefab.transform.localScale = Vector3.one * worldData.chunkSize;
        }

        public void GenerateTerrainAndBiomes()
        {
            for (int x = 0; x < worldData.worldGridSize; x++) {
                for (int y = 0; y < worldData.worldGridSize; y++) {
                    // Determine chunk's biome type
                    BiomeType biomeType = mapFeatures.GetPointBiome(x, y);

                    // Add this chunk to biomes lookup table
                    Chunk chunk = new Chunk(biomeType, new Vector2Int(x, y));
                    biomes[biomeType.GetHashCode()].chunks.Add(chunk);

                    // Spawn floor tiles if it's not Ocean
                    if (biomeType == BiomeType.Ocean) {
                        Instantiate(oceanWallCubePrefab, gridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, invisibleWallsParent);
                        continue;
                    }
                    Vector3 chunkBottomLeft = gridComponent.GetChunkBottomLeftCornerWorld(chunk);
                    for (int xx = 0; xx < worldData.floorTileGridSize; xx++) {
                        for (int yy = 0; yy < worldData.floorTileGridSize; yy++) {
                            GameObject floor = Instantiate(floorPrefab, chunkBottomLeft + new Vector3(worldData.floorTileSize * (xx + 0.5f), 0, worldData.floorTileSize * (yy + 0.5f)), Quaternion.identity, floorsParent);
                            floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;
                        }
                    }
                }
            }
        }
    }
}
