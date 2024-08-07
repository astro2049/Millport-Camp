using UnityEngine;

namespace PCG
{
    // 16 * 16 world chunk
    public class Chunk
    {
        public BiomeType biome;
        public Vector2Int cellCoord;

        public Chunk(BiomeType biome, Vector2Int cellCoord)
        {
            this.biome = biome;
            this.cellCoord = cellCoord;
        }
    }
}
