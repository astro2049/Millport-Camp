using Entities.Abilities.State;
using Entities.AI.Abilities.Gunner;
using Entities.AI.Abilities.Perception;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.CombatRobot
{
    public class CombatRobotStateComponent : StateComponent
    {
        [Header("Movement")]
        public float turnSpeed = 180f; // 180 deg/s
        public float moveSpeed = 5f;
        public float patrolPointRadius = 10f;
        public float patrolWaitTime = 3f;
        public float evadeDistance = 3f;

        public NavMeshAgent navMeshAgent;

        public GunnerComponent gunnerComponent;
        public TargetTrackerComponent targetTrackerComponent;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            gunnerComponent = GetComponent<GunnerComponent>();
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
        }

        // HFSM context
        public bool TargetIsAligned()
        {
            if (targetTrackerComponent.target) {
                Quaternion targetDirection = Quaternion.LookRotation(targetTrackerComponent.target.transform.position - transform.position);
                return Mathf.Abs(Quaternion.Angle(targetDirection, transform.rotation)) <= 1f;
            } else {
                return false;
            }
        }
    }
}
