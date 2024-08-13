using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivationDistanceComponent : MonoBehaviour
    {
        [SerializeField] private float distance = 20f;

        private void Awake()
        {
            GameObject activationColliderGameObject = new GameObject("Activation Collider") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };

            ConfigureActivationCollider(activationColliderGameObject);
        }

        private void ConfigureActivationCollider(GameObject acGo)
        {
            // Configure collider
            SphereCollider sphereCollider = acGo.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = distance;
            sphereCollider.excludeLayers = ~LayerMask.GetMask("NPC");

            // Add component: NPCActivationDistanceColliderComponent
            acGo.AddComponent<NPCActivationDistanceColliderComponent>();
        }
    }
}
