using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.CombatRobot.States
{
    public class GunHfsm : HFSMState<CombatRobotStateComponent>
    {
        public readonly TriggerFsm triggerFsm;

        public GunHfsm(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            triggerFsm = new TriggerFsm(owner, HFSMStateType.Branch, "Trigger", this);
            ReloadState reloadState = new ReloadState(owner, HFSMStateType.Leaf, "Reload", this);
            subStates.Add(triggerFsm.name, triggerFsm);
            subStates.Add(reloadState.name, reloadState);
            current = subStates["Trigger"];
        }
    }

    public class TriggerFsm : HFSMState<CombatRobotStateComponent>
    {
        public TriggerFsm(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            TriggerIdleState idleState = new TriggerIdleState(owner, HFSMStateType.Leaf, "Idle", this);
            FireState fireState = new FireState(owner, HFSMStateType.Leaf, "Fire", this);
            subStates.Add(idleState.name, idleState);
            subStates.Add(fireState.name, fireState);
            current = subStates["Idle"];
        }

        protected override void Exit()
        {
            // Upon leaving trigger state (to reload), release trigger
            ChangeState("Idle");
        }
    }

    public class ReloadState : HFSMState<CombatRobotStateComponent>
    {
        public ReloadState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Enter()
        {
            owner.gunnerComponent.Reload();
        }
    }

    public class TriggerIdleState : HFSMState<CombatRobotStateComponent>
    {
        public TriggerIdleState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState) { }

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
        public FireState(CombatRobotStateComponent owner, HFSMStateType type, string name, HFSMState<CombatRobotStateComponent> parentState) : base(owner, type, name, parentState) { }

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
