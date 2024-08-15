using Entities.Abilities.State;
using Entities.AI.Abilities.Gunner;
using Entities.AI.Abilities.Perception;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;

namespace Entities.AI.Turret
{
    public class TurretStateComponent : StateComponent
    {
        public float turnSpeed = 360f; // 360 deg/s

        public Transform baseTransform;

        public GunnerComponent gunnerComponent;
        public TargetTrackerComponent targetTrackerComponent;

        private void Awake()
        {
            gunnerComponent = GetComponent<GunnerComponent>();
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
        }

        // HFSM Context
        public bool TargetIsAligned()
        {
            if (targetTrackerComponent.target) {
                Quaternion targetRotation = Quaternion.LookRotation(targetTrackerComponent.target.transform.position - baseTransform.position);
                return Mathf.Abs(Quaternion.Angle(targetRotation, baseTransform.rotation)) <= 1f;
            } else {
                return false;
            }
        }
    }
}
