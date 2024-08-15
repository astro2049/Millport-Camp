using Entities.AI.Abilities.HFSM;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.CombatRobot.States
{
    public class MovementHfsm : HFSMState<CombatRobotStateComponent>
    {
        public readonly MovementCombatFsm combatFsm;

        public MovementHfsm(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState)
        {
            PatrolFsm patrolFsm = new PatrolFsm(owner, HFSMStateType.Branch, "Patrol", this);
            combatFsm = new MovementCombatFsm(owner, HFSMStateType.Branch, "Combat", this);
            AddSubStates(patrolFsm, combatFsm);
            current = subStates["Patrol"];
        }
    }

    public class PatrolFsm : HFSMState<CombatRobotStateComponent>
    {
        public PatrolFsm(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState)
        {
            MoveToPatrolPointState moveToPatrolPointState = new MoveToPatrolPointState(owner, HFSMStateType.Leaf, "MoveToPatrolPoint", this);
            WaitState waitState = new WaitState(owner, HFSMStateType.Leaf, "Wait", this);
            AddSubStates(moveToPatrolPointState, waitState);
            current = subStates["MoveToPatrolPoint"];
        }
    }

    public class MoveToPatrolPointState : HFSMState<CombatRobotStateComponent>
    {
        public MoveToPatrolPointState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState)
        {
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

        public WaitState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState)
        {

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
        public MovementCombatFsm(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            IdleState idleState = new IdleState(owner, HFSMStateType.Leaf, "Idle", this);
            EvadeState evadeState = new EvadeState(owner, HFSMStateType.Leaf, "Evade", this);
            AddSubStates(idleState, evadeState);
            current = subStates["Idle"];
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
        public IdleState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Execute(float deltaTime)
        {
            if (Vector3.Distance(owner.transform.position, owner.targetTrackerComponent.target.transform.position) <= owner.evadeDistance) {
                parentState.ChangeState("Evade");
            }
        }
    }

    public class EvadeState : HFSMState<CombatRobotStateComponent>
    {
        public EvadeState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState) { }

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
