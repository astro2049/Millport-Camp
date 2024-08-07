using UnityEngine;

namespace PCG
{
    public class PerlinNoise
    {
        public static float Sample(float x, float y, float offset, float scale)
        {
            return Mathf.PerlinNoise(offset + x * scale, offset + y * scale);
        }
    }
}
