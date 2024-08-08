using System;
using UnityEngine;
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

    [Serializable]
    public class WhittakerBiomes
    {
        [Header("Height")]
        [SerializeField] private float seaLevelHeight = 0.2f;
        [SerializeField] private int heightPerlinNoiseScale = 3;
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

        [Header("River")]
        [SerializeField] private Vector2 riverPerlinNoiseScale = new Vector2(10f, 10f);
        public float riverPerlinNoiseOffset;

        private int worldSize;

        // Biomes
        private BiomeType[,] biomes = {
            // Cold        // Cool           // Warm           // Hot
            /* Arid */ { BiomeType.Ice, BiomeType.Tundra, BiomeType.Desert, BiomeType.Desert },
            /* Semi-Arid */ { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland, BiomeType.Woodland },
            /* Moist */ { BiomeType.Ice, BiomeType.Taiga, BiomeType.Woodland, BiomeType.Woodland },
            /* Wet */ { BiomeType.Ice, BiomeType.Taiga, BiomeType.Woodland, BiomeType.Woodland }
        };

        public void initializeParameters(int worldSize)
        {
            // Precalculate
            temperatureDifference = equatorTemperature - northPoleTemperature;

            temperatureLevelCount = biomes.GetLength(0);
            humidityLevelCount = biomes.GetLength(1);

            temperatureInterval = temperatureDifference / temperatureLevelCount;
            humidityInterval = 1f / humidityLevelCount;

            RandomizePerlinNoisesOffsets();

            this.worldSize = worldSize;
        }

        // Randomize perlin noises sample region
        public void RandomizePerlinNoisesOffsets()
        {
            heightPerlinNoiseOffset = Random.Range(0f, 10f);
            humidityPerlinNoiseOffset = Random.Range(0f, 10f);
            riverPerlinNoiseOffset = Random.Range(0f, 10f);
        }

        public float GetHeight(int x, int y)
        {
            // Get height according to height Perlin noise map
            float height = PerlinNoise.Sample((float)x / worldSize, (float)y / worldSize, heightPerlinNoiseOffset, heightPerlinNoiseScale);
            // Apply falloff
            height -= TerrainGenerator.GetFallOffHeight(x, y, worldSize);

            return height;
        }

        public float GetTemperature(float latitude)
        {
            return equatorTemperature - temperatureDifference / worldSize * latitude + Random.Range(-5, 5);
        }

        public float GetHumidity(int x, int y)
        {
            return PerlinNoise.Sample((float)x / worldSize, (float)y / worldSize, humidityPerlinNoiseOffset, humidityPerlinNoiseScale);
        }

        public BiomeType DetermineBiome(int x, int y)
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

            return biomes[humidityLevel, temperatureLevel];
        }
    }
}
