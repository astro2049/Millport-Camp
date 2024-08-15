using Entities.AI.Abilities.Perception;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;

namespace Entities.AI.Turret
{
    public class TurretPerceptionComponent : PerceptionComponent
    {
        private TargetTrackerComponent targetTrackerComponent;

        private void Awake()
        {
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            targetTrackerComponent.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            targetTrackerComponent.RemoveTarget(other.gameObject);
        }
    }
}
