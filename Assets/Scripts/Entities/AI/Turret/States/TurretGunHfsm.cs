using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.Turret.States
{
    public class TurretGunHfsm : HFSMState<TurretStateComponent>
    {
        public TurretGunHfsm(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            TurretTriggerFsm triggerFsm = new TurretTriggerFsm(owner, HFSMStateType.Branch, "Trigger", this);
            TurretReloadState reloadState = new TurretReloadState(owner, HFSMStateType.Leaf, "Reload", this);
            AddSubStates(triggerFsm, reloadState);
            current = subStates["Trigger"];
        }
    }

    public class TurretTriggerFsm : HFSMState<TurretStateComponent>
    {
        public TurretTriggerFsm(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            TurretTriggerIdleState idleState = new TurretTriggerIdleState(owner, HFSMStateType.Leaf, "Idle", this);
            TurretFireState fireState = new TurretFireState(owner, HFSMStateType.Leaf, "Fire", this);
            AddSubStates(idleState, fireState);
            current = subStates["Idle"];
        }

        protected override void Exit()
        {
            // Upon leaving trigger state (to reload), release trigger
            ChangeState("Idle");
        }
    }

    public class TurretTriggerIdleState : HFSMState<TurretStateComponent>
    {
        public TurretTriggerIdleState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Execute(float deltaTime)
        {
            if (owner.TargetIsAligned()) {
                parentState.ChangeState("Fire");
            }
        }
    }

    // TODO: Currently the turret only works with AUTO mode guns
    public class TurretFireState : HFSMState<TurretStateComponent>
    {
        public TurretFireState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

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

    public class TurretReloadState : HFSMState<TurretStateComponent>
    {
        public TurretReloadState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Enter()
        {
            owner.gunnerComponent.Reload();
        }
    }
}
