using PCG.Generators.Terrain;
using UnityEngine;

namespace PCG.Generators
{
    public class MapFeatures : MonoBehaviour // Whittaker Biomes
    {
        [SerializeField] private WorldData worldData;

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

        public void Initialize()
        {
            InitializeParameters();
        }

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

        public float GetPointHeight(int x, int y)
        {
            // Get height according to height Perlin noise map
            float height = PerlinNoise.Sample((float)x / worldData.worldGridSize, (float)y / worldData.worldGridSize, heightPerlinNoiseOffset, heightPerlinNoiseScale);
            // Apply falloff
            height -= GetPointFallOffHeight(x, y);

            return height;
        }

        private float GetPointFallOffHeight(int x, int y)
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

        private float GetPointTemperature(float latitude)
        {
            return equatorTemperature - temperatureDifference / worldData.worldGridSize * latitude + Random.Range(-5, 5);
        }

        private float GetPointHumidity(int x, int y)
        {
            return PerlinNoise.Sample((float)x / worldData.worldGridSize, (float)y / worldData.worldGridSize, humidityPerlinNoiseOffset, humidityPerlinNoiseScale);
        }

        public BiomeType GetPointBiome(int x, int y)
        {
            if (GetPointHeight(x, y) <= seaLevelHeight) {
                return BiomeType.Ocean;
            }

            float temperature = GetPointTemperature(y);
            int temperatureLevel = (int)((temperature - northPoleTemperature) / temperatureInterval);
            // Clamp temperature level because there's fluctuation
            temperatureLevel = Mathf.Clamp(temperatureLevel, 0, temperatureLevelCount - 1);

            float humidity = GetPointHumidity(x, y);
            int humidityLevel = (int)(humidity / humidityInterval);
            // Clamp humidity level because there's fluctuation
            humidityLevel = Mathf.Clamp(humidityLevel, 0, humidityLevelCount - 1);

            return whittakerBiomes[humidityLevel, temperatureLevel];
        }
    }
}
