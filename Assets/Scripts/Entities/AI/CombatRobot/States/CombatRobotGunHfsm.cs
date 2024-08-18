using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.CombatRobot.States
{
    public class GunHfsm : HFSMState<CombatRobotStateComponent>
    {
        public GunHfsm(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Gun";

            TriggerFsm triggerFsm = new TriggerFsm(owner, this);
            ReloadState reloadState = new ReloadState(owner, this);
            AddSubStates(triggerFsm, reloadState);
            current = triggerFsm;
        }
    }

    public class TriggerFsm : HFSMState<CombatRobotStateComponent>
    {
        public TriggerFsm(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Trigger";

            TriggerIdleState idleState = new TriggerIdleState(owner, this);
            FireState fireState = new FireState(owner, this);
            AddSubStates(idleState, fireState);
            current = idleState;
        }

        protected override void Exit()
        {
            // Upon leaving trigger state (to reload), release trigger
            ChangeState("Idle");
        }
    }

    public class ReloadState : HFSMState<CombatRobotStateComponent>
    {
        public ReloadState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Reload";
        }

        protected override void Enter()
        {
            owner.gunnerComponent.Reload();
        }
    }

    public class TriggerIdleState : HFSMState<CombatRobotStateComponent>
    {
        public TriggerIdleState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Idle";
        }

        protected override void Execute(float deltaTime)
        {
            if (owner.TargetIsAligned()) {
                parentState.ChangeState("Fire");
            }
        }
    }

    // TODO: Currently only works with AUTO mode guns
    public class FireState : HFSMState<CombatRobotStateComponent>
    {
        public FireState(CombatRobotStateComponent owner, HFSMState<CombatRobotStateComponent> parentState) : base(owner, parentState)
        {
            name = "Fire";
        }

        protected override void Enter()
        {
            owner.gunnerComponent.TriggerDown();
        }

        protected override void Execute(float deltaTime)
        {
            if (!owner.TargetIsAligned()) {
                parentState.ChangeState("Idle");
                return;
            }
            // Set gun look point to target's chest center
            owner.gunnerComponent.gun.lookPoint = owner.targetTrackerComponent.target.transform.position + new Vector3(0, 1, 0);
        }

        protected override void Exit()
        {
            owner.gunnerComponent.TriggerUp();
        }
    }
}
