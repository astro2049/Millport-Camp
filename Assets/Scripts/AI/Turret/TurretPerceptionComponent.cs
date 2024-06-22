using UnityEngine;

namespace AI.Turret
{
    public class TurretPerceptionComponent : PerceptionComponent
    {
        private TurretHFSMComponent turretHfsm;

        private void Awake()
        {
            turretHfsm = GetComponent<TurretHFSMComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            turretHfsm.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            turretHfsm.RemoveTarget(other.gameObject);
        }
    }
}
