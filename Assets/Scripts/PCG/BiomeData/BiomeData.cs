using System;
using UnityEngine;

namespace PCG.BiomeData
{
    [Serializable]
    public class FoliageConfig
    {
        [Header("Possibility")]
        [Range(0f, 1f)] public float occurence = 1f;
        [Header("Density")]
        [Range(1, 16)] public int subCellCount;
        [Header("Size Range")]
        [Range(0.3f, 1f)] public float minSize = 0.5f;
        [Range(0.5f, 2f)] public float maxSize = 1.5f;
        [Header("Assets")]
        public GameObject[] prefabs;
    }

    [CreateAssetMenu(fileName = "Biome Data", menuName = "Scriptable Objects/Biome Data", order = 2)]
    public class BiomeData : ScriptableObject
    {
        public Material floorMaterial;
        public FoliageConfig[] foliageConfigs;
    }
}
