using System;
using UnityEngine;

namespace PCG.BiomeData
{
    [Serializable]
    public class FoliageConfig
    {
        [Header("Density")]
        [Range(1, 16)] public int subCellCount;
        [Header("Size Range")]
        [Range(0.3f, 1f)] public float minSize = 0.5f;
        [Range(1f, 2f)] public float maxSize = 1.5f;
        [Header("Assets")]
        public GameObject[] prefabs;
    }

    [CreateAssetMenu(fileName = "BiomeData", menuName = "Scriptable Objects/BiomeData", order = 2)]
    public class BiomeData : ScriptableObject
    {
        public Material floorMaterial;
        public FoliageConfig[] foliageConfigs;
    }
}
