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
        Desert = 6
    }

    [Serializable]
    public class WhittakerBiomes
    {
        [Header("Temperature")]
        [SerializeField] private float equatorTemperature = 30f;
        [SerializeField] private float northPoleTemperature = -10f;
        private float temperatureDifference;
        private float temperatureInterval;
        private int temperatureLevelCount;

        private float humidityInterval;
        private int humidityLevelCount;

        [Header("Perlin Noise")]
        [SerializeField] private int perlinNoiseScale = 10;
        public float perlinNoiseOffset;

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

            // Randomize perlin noise sample region
            perlinNoiseOffset = Random.Range(0, 10);

            this.worldSize = worldSize;
        }

        public float GetTemperature(float latitude)
        {
            return equatorTemperature - temperatureDifference / worldSize * latitude + Random.Range(-5, 5);
        }

        public float GetHumidity(int x, int y)
        {
            return Mathf.PerlinNoise(perlinNoiseOffset + (float)x / worldSize * 10, perlinNoiseOffset + (float)y / worldSize * perlinNoiseScale);
        }

        public BiomeType DetermineBiome(int x, int y)
        {
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
