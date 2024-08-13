﻿using UnityEngine;

namespace PCG
{
    // 16 * 16 world chunk
    public class Chunk
    {
        public BiomeType biome;
        public Vector2Int cellCoordinate;

        public Chunk(BiomeType biome, Vector2Int cellCoordinate)
        {
            this.biome = biome;
            this.cellCoordinate = cellCoordinate;
        }
    }
}
