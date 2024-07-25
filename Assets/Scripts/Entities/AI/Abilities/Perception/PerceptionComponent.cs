using UnityEngine;

namespace Entities.AI.Abilities.Perception
{
    public abstract class PerceptionComponent : MonoBehaviour
    {
        public void CreateSensorCollider(float radius, LayerMask[] includeLayers)
        {
            GameObject sensorGameObject = new GameObject("Sensor Collider");
            sensorGameObject.transform.parent = transform;
            sensorGameObject.transform.localPosition = Vector3.zero;

            /*
             * Configure sensor collider
             */
            SphereCollider sensorCollider = sensorGameObject.AddComponent<SphereCollider>();
            sensorCollider.isTrigger = true;
            sensorCollider.radius = radius;
            // Layer masks
            LayerMask combinedLayerMask = 0;
            foreach (LayerMask layer in includeLayers) {
                combinedLayerMask |= layer;
            }
            sensorCollider.includeLayers = combinedLayerMask;
            sensorCollider.excludeLayers = ~combinedLayerMask;

            sensorGameObject.AddComponent<SensorComponent>();
        }

        public abstract void OnPerceptionTriggerEnter(Collider other);

        public abstract void OnPerceptionTriggerExit(Collider other);
    }
}