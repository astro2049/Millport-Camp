using UnityEngine;

namespace PCG
{
    public class TerrainGenerator
    {
        public static float GetFallOffHeight(int x, int y, int worldSize)
        {
            float xDistance = Mathf.Abs(x - worldSize / 2f);
            float yDistance = Mathf.Abs(y - worldSize / 2f);

            // Distance from midpoint, pick x/y max deviation
            float deviation = Mathf.Max(xDistance, yDistance);

            // Calculate falloff
            // Normalize deviation, range from 0 to 1
            float normalizedDeviation = deviation / (worldSize / 2f);
            float falloff = Mathf.Pow(normalizedDeviation, 4);

            return falloff;
        }
    }
}
