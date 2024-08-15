using UnityEngine;

namespace Entities.AI.Abilities.Perception
{
    public abstract class PerceptionComponent : MonoBehaviour
    {
        [Header("Perception")]
        public float radius;
        public LayerMask includeLayers;

        protected void CreateSensorCollider()
        {
            GameObject sensorGameObject = new GameObject("Sensor Collider") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };

            // Configure sensor collider
            SphereCollider sensorCollider = sensorGameObject.AddComponent<SphereCollider>();
            sensorCollider.isTrigger = true;
            sensorCollider.radius = radius;
            sensorCollider.excludeLayers = ~includeLayers;

            sensorGameObject.AddComponent<SensorComponent>();
        }

        public abstract void OnPerceptionTriggerEnter(Collider other);

        public abstract void OnPerceptionTriggerExit(Collider other);
    }
}
