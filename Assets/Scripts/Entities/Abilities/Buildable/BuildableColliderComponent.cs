using UnityEngine;

namespace Entities.Abilities.Buildable
{
    public class BuildableColliderComponent : MonoBehaviour
    {
        private BuildableComponent buildableComponent;
        private int overlappingGameObjectsCount = 0;

        private void Awake()
        {
            enabled = false;
        }

        public void init(BuildableComponent buildableComponent)
        {
            this.buildableComponent = buildableComponent;
            enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignores the clearing distance collider on the main camera
            if (other.CompareTag("MainCamera")) {
                return;
            }

            overlappingGameObjectsCount++;
            if (overlappingGameObjectsCount == 1) {
                buildableComponent.SetIsOkToPlace(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Ignores the clearing distance collider on the main camera
            if (other.CompareTag("MainCamera")) {
                return;
            }

            overlappingGameObjectsCount--;
            if (overlappingGameObjectsCount == 0) {
                buildableComponent.SetIsOkToPlace(true);
            }
        }
    }
}
