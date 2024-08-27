using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActivateeComponent : MonoBehaviour
    {
        private void Awake()
        {
            // Create parent game object
            GameObject parent = new GameObject(gameObject.name) {
                transform = {
                    position = transform.position
                }
            };
            ActivateeSwitchComponent activateeSwitchComponent = parent.AddComponent<ActivateeSwitchComponent>();
            activateeSwitchComponent.actor = gameObject;
            activateeSwitchComponent.Deactivate();

            // Reorder parents
            gameObject.name = "Actor";
            parent.transform.parent = transform.parent;
            transform.parent = parent.transform;

            // Create activation collider game object (same level)
            GameObject activationCollider = new GameObject("Activation Collider") {
                layer = LayerMask.NameToLayer("Activation"),
                transform = {
                    parent = parent.transform,
                    localPosition = Vector3.zero
                }
            };
            SphereCollider sphereCollider = activationCollider.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
        }
    }
}
