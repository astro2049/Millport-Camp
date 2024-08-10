using UnityEngine;

namespace Entities.Abilities.ClearingDistance
{
    public class MaterialShifterComponent : MonoBehaviour
    {
        private Material material;

        private void Awake()
        {
            material = GetComponent<MeshRenderer>().material;
        }

        public void MakeTransparent(Material transparentMaterial)
        {
            GetComponent<MeshRenderer>().material = transparentMaterial;
        }

        public void RestoreMaterials()
        {
            GetComponent<MeshRenderer>().material = material;
        }
    }
}
