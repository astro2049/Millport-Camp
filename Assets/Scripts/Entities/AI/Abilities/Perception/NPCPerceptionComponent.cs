using Entities.AI.Abilities.TargetTracker;
using UnityEngine;

namespace Entities.AI.Abilities.Perception
{
    public class NPCPerceptionComponent : PerceptionComponent
    {
        private TargetTrackerComponent targetTrackerComponent;

        private void Awake()
        {
            // Configure perception
            CreateSensorCollider();

            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            // Ignore own kind
            if (other.gameObject.CompareTag(gameObject.tag)) {
                return;
            }
            targetTrackerComponent.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            // Ignore own kind
            if (other.gameObject.CompareTag(gameObject.tag)) {
                return;
            }
            targetTrackerComponent.RemoveTarget(other.gameObject);
        }
    }
}
