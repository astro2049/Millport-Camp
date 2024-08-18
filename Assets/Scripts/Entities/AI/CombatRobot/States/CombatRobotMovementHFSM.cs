using Entities.AI.Abilities.HFSM;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.CombatRobot.States
{
    public class MovementHfsm : HFSMState<CombatRobotStateComponent>
    {
        public readonly MovementCombatFsm combatFsm;

        public MovementHfsm(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Fire";

            PatrolFsm patrolFsm = new PatrolFsm(owner, this);
            combatFsm = new MovementCombatFsm(owner, this);
            AddSubStates(patrolFsm, combatFsm);
            current = patrolFsm;
        }
    }

    public class PatrolFsm : HFSMState<CombatRobotStateComponent>
    {
        public PatrolFsm(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Patrol";

            MoveToPatrolPointState moveToPatrolPointState = new MoveToPatrolPointState(owner, this);
            WaitState waitState = new WaitState(owner, this);
            AddSubStates(moveToPatrolPointState, waitState);
            current = moveToPatrolPointState;
        }
    }

    public class MoveToPatrolPointState : HFSMState<CombatRobotStateComponent>
    {
        public MoveToPatrolPointState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "MoveToPatrolPoint";
        }

        protected override void Enter()
        {
            Vector3 patrolPoint = GeneratePatrolPoint();
            owner.navMeshAgent.SetDestination(patrolPoint);
        }

        private Vector3 GeneratePatrolPoint()
        {
            NavMesh.SamplePosition(owner.transform.position + Random.insideUnitSphere * owner.patrolPointRadius, out NavMeshHit hit, 10, 1);
            return hit.position;
        }

        protected override void Execute(float deltaTime)
        {
            if (!owner.navMeshAgent.pathPending && owner.navMeshAgent.remainingDistance <= owner.navMeshAgent.stoppingDistance) {
                parentState.ChangeState("Wait");
            }
        }

        protected override void Exit()
        {
            owner.navMeshAgent.ResetPath();
        }
    }

    public class WaitState : HFSMState<CombatRobotStateComponent>
    {
        private float timer;

        public WaitState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Wait";
        }

        protected override void Enter()
        {
            timer = 0;
        }

        protected override void Execute(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= owner.patrolWaitTime) {
                parentState.ChangeState("MoveToPatrolPoint");
            }
        }
    }

    public class MovementCombatFsm : HFSMState<CombatRobotStateComponent>
    {
        public MovementCombatFsm(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Combat";

            IdleState idleState = new IdleState(owner, this);
            EvadeState evadeState = new EvadeState(owner, this);
            AddSubStates(idleState, evadeState);
            current = idleState;
        }

        protected override void Execute(float deltaTime)
        {
            // Turn to face the target enemy
            Quaternion targetRotation = Quaternion.LookRotation(owner.targetTrackerComponent.target.transform.position - owner.transform.position);
            owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation, targetRotation, owner.turnSpeed * Time.deltaTime);
        }
    }

    public class IdleState : HFSMState<CombatRobotStateComponent>
    {
        public IdleState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Idle";
        }

        protected override void Execute(float deltaTime)
        {
            if (Vector3.Distance(owner.transform.position, owner.targetTrackerComponent.target.transform.position) <= owner.evadeDistance) {
                parentState.ChangeState("Evade");
            }
        }
    }

    public class EvadeState : HFSMState<CombatRobotStateComponent>
    {
        public EvadeState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Evade";
        }

        protected override void Enter()
        {
            owner.navMeshAgent.enabled = false;
        }

        protected override void Execute(float deltaTime)
        {
            // Back off at movement speed
            owner.transform.Translate(Vector3.Normalize(owner.transform.position - owner.targetTrackerComponent.target.transform.position) * owner.moveSpeed * deltaTime, Space.World);
            if (Vector3.Distance(owner.transform.position, owner.targetTrackerComponent.target.transform.position) >= owner.evadeDistance) {
                parentState.ChangeState("Idle");
            }
        }

        protected override void Exit()
        {
            owner.navMeshAgent.enabled = true;
        }
    }
}
