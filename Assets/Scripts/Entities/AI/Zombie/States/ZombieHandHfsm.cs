using Entities.AI.Abilities.HFSM;

namespace Entities.AI.Zombie.States
{
    public class HandHfsm : HFSMState<ZombieStateComponent>
    {
        public HandHfsm(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {
            IdleState idleState = new IdleState(owner, HFSMStateType.Leaf, "Idle", this);
            AttackFsm attackFsm = new AttackFsm(owner, HFSMStateType.Branch, "Attack", this);
            AddSubStates(idleState, attackFsm);
            current = subStates["Idle"];
        }
    }

    public class IdleState : HFSMState<ZombieStateComponent>
    {
        public IdleState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }
    }

    public class AttackFsm : HFSMState<ZombieStateComponent>
    {
        public AttackFsm(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {
            AttackState attackState = new AttackState(owner, HFSMStateType.Leaf, "Attack", this);
            AttackWaitState attackWaitState = new AttackWaitState(owner, HFSMStateType.Leaf, "Wait", this);
            AddSubStates(attackState, attackWaitState);
            current = subStates["Attack"];
        }
    }

    public class AttackState : HFSMState<ZombieStateComponent>
    {
        public AttackState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }

        protected override void Execute(float deltaTime)
        {
            owner.damageDealerComponent.DealDamage(owner.targetTrackerComponent.target, owner.attackDamage);
            parentState.ChangeState("Wait");
        }
    }

    public class AttackWaitState : HFSMState<ZombieStateComponent>
    {
        private float timer;

        public AttackWaitState(ZombieStateComponent owner, HFSMStateType type, string name, HFSMState<ZombieStateComponent> parentState) : base(owner, type, name, parentState)
        {

        }

        protected override void Enter()
        {
            timer = 0;
        }

        protected override void Execute(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= owner.attackInterval) {
                parentState.ChangeState("Attack");
            }
        }
    }
}
