using UnityEngine;

namespace Entities.Ocean
{
    public class OceanComponent : MonoBehaviour
    {
        public void Resize(float size)
        {
            transform.localScale = new Vector3(size / 10f, 1, size / 10f);

            // Adjust ocean's ripple density according to world size
            // TODO: Kind of hacky. Also, 4 here is relative size of PCG map to testing ground
            float rippleDensity = GetComponent<MeshRenderer>().material.GetFloat("_RippleDensity");
            GetComponent<MeshRenderer>().material.SetFloat("_RippleDensity", rippleDensity * transform.localScale.x / 12.8f);
        }
    }
}
