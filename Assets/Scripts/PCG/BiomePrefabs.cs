using UnityEngine;

namespace PCG
{
    [CreateAssetMenu(fileName = "BiomeData", menuName = "Scriptable Objects/BiomeData", order = 2)]
    public class BiomeData : ScriptableObject
    {
        public GameObject[] floors;
        public GameObject[] trees;
        public GameObject[] rocks;
        public GameObject[] flowers;
        public GameObject[] hills;
    }
}
