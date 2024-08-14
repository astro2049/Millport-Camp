using System.Collections.Generic;
using System.Linq;
using Entities.Abilities.Observer;
using Entities.Abilities.State;
using Entities.AI.Abilities.Gunner;
using Entities.AI.Abilities.Perception;
using UnityEngine;
using UnityEngine.AI;
using EventType = Entities.Abilities.Observer.EventType;

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

        [Header("Perception")]
        public float perceptionRadius = 12f;
        public LayerMask perceptionLayers;

        public NavMeshAgent navMeshAgent;

        public GunnerComponent gunnerComponent;

        private CombatRobotHFSMComponent combatRobotHfsm;
        private CombatRobotObserverComponent combatRobotObserverComponent;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            gunnerComponent = GetComponent<GunnerComponent>();
            combatRobotHfsm = GetComponent<CombatRobotHFSMComponent>();
            combatRobotObserverComponent = GetComponent<CombatRobotObserverComponent>();

            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);
        }

        // HFSM context
        public GameObject target;
        public readonly HashSet<GameObject> enemies = new HashSet<GameObject>();

        public void AddTarget(GameObject enemy)
        {
            // If there's no target lock-on, set target
            if (!target) {
                target = enemy;
                // Transition: Patrol -> Combat
                combatRobotHfsm.movementHfsm.ChangeState("Combat");
            }

            // Add enemy to enemies list (set),
            // and subscribe to its event: PawnDead
            enemies.Add(enemy);
            enemy.GetComponent<SubjectComponent>().AddObserver(combatRobotObserverComponent, EventType.PawnDead);
        }

        public void RemoveTarget(GameObject enemy)
        {
            // Remove enemy lost track of off the enemy list (set)
            enemies.Remove(enemy);

            // Assign new target
            if (enemies.Count != 0) {
                // If the enemy list is not empty, assign first in set as new target
                // TODO: This can be more intelligent, like selecting based on distance
                target = enemies.First();
            } else {
                // If the enemy list is empty, then there's no target at the moment
                target = null;
                // Transition: Combat -> Patrol
                combatRobotHfsm.movementHfsm.ChangeState("Patrol");
            }
        }

        public bool TargetIsAligned()
        {
            if (target) {
                Quaternion targetDirection = Quaternion.LookRotation(target.transform.position - transform.position);
                return Mathf.Abs(Quaternion.Angle(targetDirection, transform.rotation)) <= 1f;
            } else {
                return false;
            }
        }
    }
}
