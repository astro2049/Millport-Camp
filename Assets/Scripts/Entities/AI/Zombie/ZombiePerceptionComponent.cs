using Entities.AI.Abilities.Perception;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;

namespace Entities.AI.Zombie
{
    public class ZombiePerceptionComponent : PerceptionComponent
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
            if (other.gameObject.CompareTag("Zombie")) {
                return;
            }
            targetTrackerComponent.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Zombie")) {
                return;
            }
            targetTrackerComponent.RemoveTarget(other.gameObject);
        }
    }
}
