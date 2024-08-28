using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.Turret.States
{
    public class TurretGunHfsm : HFSMState<TurretStateComponent>
    {
        public TurretGunHfsm(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Gun";

            TurretTriggerFsm triggerFsm = new TurretTriggerFsm(owner, this);
            TurretReloadState reloadState = new TurretReloadState(owner, this);
            AddSubStates(triggerFsm, reloadState);
            current = triggerFsm;
        }
    }

    public class TurretTriggerFsm : HFSMState<TurretStateComponent>
    {
        public TurretTriggerFsm(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Trigger";

            TurretTriggerIdleState idleState = new TurretTriggerIdleState(owner, this);
            TurretFireState fireState = new TurretFireState(owner, this);
            AddSubStates(idleState, fireState);
            current = idleState;
        }

        protected override void Exit()
        {
            // Upon leaving trigger state (to reload), release trigger
            ChangeState("Idle");
        }
    }

    public class TurretTriggerIdleState : HFSMState<TurretStateComponent>
    {
        public TurretTriggerIdleState(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState) : base(owner, parentState)
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

    // TODO: Currently the turret only works with AUTO mode guns
    public class TurretFireState : HFSMState<TurretStateComponent>
    {
        public TurretFireState(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState) : base(owner, parentState)
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
            owner.gunnerComponent.gun.transform.rotation = Quaternion.LookRotation(owner.targetTrackerComponent.target.transform.position + new Vector3(0, 1, 0) - owner.gunnerComponent.gun.transform.position);
        }

        protected override void Exit()
        {
            owner.gunnerComponent.TriggerUp();
        }
    }

    public class TurretReloadState : HFSMState<TurretStateComponent>
    {
        public TurretReloadState(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState) : base(owner, parentState)
        {
            name = "Reload";
        }

        protected override void Enter()
        {
            owner.gunnerComponent.Reload();
        }
    }
}
