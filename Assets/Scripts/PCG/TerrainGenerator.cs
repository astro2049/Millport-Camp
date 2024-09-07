using System;
using PCG.Chunks;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace PCG
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
        private ChunkGridComponent chunkGridComponent;

        private Biome[] biomes;

        [SerializeField] private Transform floorsParent;
        [SerializeField] private Transform invisibleWallsParent;

        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject oceanWallCubePrefab;

        public void Initialize()
        {
            chunkGridComponent = GetComponent<ChunkGridComponent>();
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

            // Initialize biomes parameters
            InitializeParameters();
        }

        public void GenerateTerrainAndBiomes()
        {
            for (int x = 0; x < worldData.worldGridSize; x++) {
                for (int y = 0; y < worldData.worldGridSize; y++) {
                    // Determine chunk's biome type
                    BiomeType biomeType = DetermineBiome(x, y);

                    // Add this chunk to biomes lookup table
                    Chunk chunk = new Chunk(biomeType, new Vector2Int(x, y));
                    biomes[biomeType.GetHashCode()].chunks.Add(chunk);

                    // Spawn floor tiles if it's not Ocean
                    if (biomeType == BiomeType.Ocean) {
                        Instantiate(oceanWallCubePrefab, chunkGridComponent.GetChunkCenterWorld(chunk), Quaternion.identity, invisibleWallsParent);
                        continue;
                    }
                    Vector3 chunkBottomLeft = chunkGridComponent.GetChunkBottomLeftCornerWorld(chunk);
                    for (int xx = 0; xx < worldData.floorTileGridSize; xx++) {
                        for (int yy = 0; yy < worldData.floorTileGridSize; yy++) {
                            GameObject floor = Instantiate(floorPrefab, chunkBottomLeft + new Vector3(worldData.floorTileSize * (xx + 0.5f), 0, worldData.floorTileSize * (yy + 0.5f)), Quaternion.identity, floorsParent);
                            floor.GetComponent<MeshRenderer>().material = biomes[biomeType.GetHashCode()].biomeData.floorMaterial;
                        }
                    }
                }
            }
        }

        /*
         * Whittaker Biomes
         */
        [Header("Height")]
        [SerializeField] private float seaLevelHeight = 0.15f;
        [SerializeField] private int heightPerlinNoiseScale = 5;
        public float heightPerlinNoiseOffset;

        [Header("Temperature")]
        [SerializeField] private float equatorTemperature = 30f;
        [SerializeField] private float northPoleTemperature = -10f;
        private float temperatureDifference;
        private float temperatureInterval;
        private int temperatureLevelCount;

        [Header("Humidity")]
        [SerializeField] private int humidityPerlinNoiseScale = 4;
        public float humidityPerlinNoiseOffset;
        private float humidityInterval;
        private int humidityLevelCount;

        // Biomes
        private readonly BiomeType[,] whittakerBiomes = {
            // Cold        // Cool           // Warm           // Hot
            /* Arid */ { BiomeType.Ice, BiomeType.Tundra, BiomeType.Desert, BiomeType.Desert },
            /* Semi-Arid */ { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland, BiomeType.Woodland },
            /* Moist */ { BiomeType.Ice, BiomeType.Taiga, BiomeType.Woodland, BiomeType.Woodland },
            /* Wet */ { BiomeType.Ice, BiomeType.Taiga, BiomeType.Woodland, BiomeType.Woodland }
        };

        private void InitializeParameters()
        {
            // Precalculate
            temperatureDifference = equatorTemperature - northPoleTemperature;

            temperatureLevelCount = whittakerBiomes.GetLength(0);
            humidityLevelCount = whittakerBiomes.GetLength(1);

            temperatureInterval = temperatureDifference / temperatureLevelCount;
            humidityInterval = 1f / humidityLevelCount;

            // Randomize perlin noise maps' sample regions
            heightPerlinNoiseOffset = Random.Range(0f, 10f);
            humidityPerlinNoiseOffset = Random.Range(0f, 10f);
        }

        private float GetHeight(int x, int y)
        {
            // Get height according to height Perlin noise map
            float height = PerlinNoise.Sample((float)x / worldData.worldGridSize, (float)y / worldData.worldGridSize, heightPerlinNoiseOffset, heightPerlinNoiseScale);
            // Apply falloff
            height -= GetFallOffHeight(x, y);

            return height;
        }

        private float GetFallOffHeight(int x, int y)
        {
            float xDistance = Mathf.Abs(x - worldData.worldGridSize / 2f);
            float yDistance = Mathf.Abs(y - worldData.worldGridSize / 2f);

            // Distance from midpoint, pick x/y max deviation
            float deviation = Mathf.Max(xDistance, yDistance);

            // Calculate falloff
            // Normalize deviation, range from 0 to 1
            float normalizedDeviation = deviation / (worldData.worldGridSize / 2f);
            float falloff = Mathf.Pow(normalizedDeviation, 4);

            return falloff;
        }

        private float GetTemperature(float latitude)
        {
            return equatorTemperature - temperatureDifference / worldData.worldGridSize * latitude + Random.Range(-5, 5);
        }

        private float GetHumidity(int x, int y)
        {
            return PerlinNoise.Sample((float)x / worldData.worldGridSize, (float)y / worldData.worldGridSize, humidityPerlinNoiseOffset, humidityPerlinNoiseScale);
        }

        private BiomeType DetermineBiome(int x, int y)
        {
            if (GetHeight(x, y) <= seaLevelHeight) {
                return BiomeType.Ocean;
            }

            float temperature = GetTemperature(y);
            int temperatureLevel = (int)((temperature - northPoleTemperature) / temperatureInterval);
            // Clamp temperature level because there's fluctuation
            temperatureLevel = Mathf.Clamp(temperatureLevel, 0, temperatureLevelCount - 1);

            float humidity = GetHumidity(x, y);
            int humidityLevel = (int)(humidity / humidityInterval);
            // Clamp humidity level because there's fluctuation
            humidityLevel = Mathf.Clamp(humidityLevel, 0, humidityLevelCount - 1);

            return whittakerBiomes[humidityLevel, temperatureLevel];
        }
    }
}
