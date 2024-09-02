using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActivateeComponent : MonoBehaviour
    {
        private void Awake()
        {
            // 0. Create parent game object
            GameObject parent = new GameObject(gameObject.name) {
                transform = {
                    position = transform.position
                }
            };
            // Reorder parents
            gameObject.name = "Actor";
            parent.transform.parent = transform.parent;
            transform.parent = parent.transform;

            // 1. Create activation collider game object (same level)
            GameObject activationColliderGo = new GameObject("Activation Collider") {
                layer = LayerMask.NameToLayer("Activation"),
                transform = {
                    parent = parent.transform,
                    localPosition = Vector3.zero
                }
            };
            // Collider
            SphereCollider sphereCollider = activationColliderGo.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            // Switch
            ActivateeSwitchComponent activateeSwitch = activationColliderGo.AddComponent<ActivateeSwitchComponent>();
            activateeSwitch.actor = gameObject;
            activateeSwitch.Deactivate();
        }
    }
}
