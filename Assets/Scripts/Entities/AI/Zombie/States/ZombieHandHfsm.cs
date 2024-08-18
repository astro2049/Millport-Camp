using Entities.AI.Abilities.HFSM;

namespace Entities.AI.Zombie.States
{
    public class HandHfsm : HFSMState<ZombieStateComponent>
    {
        public HandHfsm(ZombieStateComponent owner, HFSMState<ZombieStateComponent> parentState) : base(owner, parentState)
        {
            name = "Hand";

            IdleState idleState = new IdleState(owner, this);
            AttackFsm attackFsm = new AttackFsm(owner, this);
            AddSubStates(idleState, attackFsm);
            current = idleState;
        }
    }

    public class IdleState : HFSMState<ZombieStateComponent>
    {
        public IdleState(ZombieStateComponent owner, HFSMState<ZombieStateComponent> parentState) : base(owner, parentState)
        {
            name = "Idle";
        }
    }

    public class AttackFsm : HFSMState<ZombieStateComponent>
    {
        public AttackFsm(ZombieStateComponent owner, HFSMState<ZombieStateComponent> parentState) : base(owner, parentState)
        {
            name = "Attack";

            AttackState attackState = new AttackState(owner, this);
            AttackWaitState attackWaitState = new AttackWaitState(owner, this);
            AddSubStates(attackState, attackWaitState);
            current = attackState;
        }
    }

    public class AttackState : HFSMState<ZombieStateComponent>
    {
        public AttackState(ZombieStateComponent owner, HFSMState<ZombieStateComponent> parentState) : base(owner, parentState)
        {
            name = "Attack";
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

        public AttackWaitState(ZombieStateComponent owner, HFSMState<ZombieStateComponent> parentState) : base(owner, parentState)
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
            if (timer >= owner.attackInterval) {
                parentState.ChangeState("Attack");
            }
        }
    }
}
