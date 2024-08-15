using Entities.AI.Abilities.HFSM;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Zombie.States
{
    public class MovementHfsm : HFSMState<ZombieStateComponent>
    {
        public MovementHfsm(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {
            PatrolFsm patrolFsm = new PatrolFsm(owner, HFSMStateType.Branch, "Patrol", this);
            ChaseState chaseState = new ChaseState(owner, HFSMStateType.Leaf, "Chase", this);
            AddSubStates(patrolFsm, chaseState);
            current = subStates["Patrol"];
        }
    }

    public class PatrolFsm : HFSMState<ZombieStateComponent>
    {
        public PatrolFsm(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {
            MoveToPatrolPointState moveToPatrolPointState = new MoveToPatrolPointState(owner, HFSMStateType.Leaf, "MoveToPatrolPoint", this);
            PatrolWaitState waitState = new PatrolWaitState(owner, HFSMStateType.Leaf, "Wait", this);
            AddSubStates(moveToPatrolPointState, waitState);
            current = subStates["MoveToPatrolPoint"];
        }
    }

    public class MoveToPatrolPointState : HFSMState<ZombieStateComponent>
    {
        public MoveToPatrolPointState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }

        protected override void Enter()
        {
            owner.navMeshAgent.speed = owner.roamSpeed;
            Vector3 patrolPoint = GeneratePatrolPoint();
            owner.navMeshAgent.SetDestination(patrolPoint);
        }

        private Vector3 GeneratePatrolPoint()
        {
            NavMesh.SamplePosition(owner.transform.position + Random.insideUnitSphere * owner.roamPointRadius, out NavMeshHit hit, 10, 1);
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

    public class PatrolWaitState : HFSMState<ZombieStateComponent>
    {
        private float timer;

        public PatrolWaitState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }

        protected override void Enter()
        {
            timer = 0;
        }

        protected override void Execute(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= owner.roamWaitTime) {
                parentState.ChangeState("MoveToPatrolPoint");
            }
        }
    }

    public class ChaseState : HFSMState<ZombieStateComponent>
    {
        public ChaseState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }

        protected override void Enter()
        {
            owner.navMeshAgent.speed = owner.chaseSpeed;
        }

        protected override void Execute(float deltaTime)
        {
            owner.navMeshAgent.SetDestination(owner.targetTrackerComponent.target.transform.position);
            if (Vector3.Distance(owner.transform.position, owner.targetTrackerComponent.target.transform.position) <= owner.attackRange) {
                if (owner.hfsmComponent.handHfsm.current.name == "Idle") {
                    owner.hfsmComponent.handHfsm.ChangeState("Attack");
                }
            } else {
                if (owner.hfsmComponent.handHfsm.current.name == "Attack") {
                    owner.hfsmComponent.handHfsm.ChangeState("Idle");
                }
            }
        }
    }
}
