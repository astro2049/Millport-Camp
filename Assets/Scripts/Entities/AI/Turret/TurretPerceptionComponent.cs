using UnityEngine;

namespace Entities.AI.Turret
{
    public class TurretPerceptionComponent : PerceptionComponent
    {
        private TurretStateComponent turretStateComponent;

        private void Awake()
        {
            turretStateComponent = GetComponent<TurretStateComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            turretStateComponent.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            turretStateComponent.RemoveTarget(other.gameObject);
        }
    }
}
