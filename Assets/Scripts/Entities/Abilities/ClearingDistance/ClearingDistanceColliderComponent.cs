using UnityEngine;

namespace Entities.Abilities.ClearingDistance
{
    public class ClearingDistanceColliderComponent : MonoBehaviour
    {
        [SerializeField] private Material transparentStructureMaterial;

        private void Awake()
        {
            ConfigureCollider(GetComponent<Camera>().orthographicSize);
        }

        public void ConfigureCollider(float orthoSize)
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(orthoSize, orthoSize, orthoSize / Mathf.Sin(transform.rotation.eulerAngles.x * Mathf.PI / 180));
            boxCollider.center = new Vector3(0, 0, boxCollider.size.z / 2);
        }

        private void OnTriggerEnter(Collider other)
        {
            MaterialShifterComponent materialShifterComponent = other.gameObject.GetComponent<MaterialShifterComponent>();
            // Turret does not have this component, foliage does
            if (materialShifterComponent) {
                materialShifterComponent.MakeTransparent(transparentStructureMaterial);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            MaterialShifterComponent materialShifterComponent = other.gameObject.GetComponent<MaterialShifterComponent>();
            // Turret does not have this component, foliage does
            if (materialShifterComponent) {
                materialShifterComponent.RestoreMaterials();
            }
        }
    }
}
