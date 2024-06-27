using AI.HFSM;
using UnityEngine;

namespace AI.Turret.States
{
    /*
     * Turret gun HFSM leaf/action states
     */
    // Idle
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

    // Fire
    // TODO: Currently the turret only works with AUTO mode guns
    public class TurretFireState : HFSMState<TurretStateComponent>
    {
        public TurretFireState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Enter()
        {
            owner.TriggerDown();
        }

        protected override void Execute(float deltaTime)
        {
            if (!owner.TargetIsAligned()) {
                parentState.ChangeState("Idle");
                return;
            }
            // Set gun look point to target's chest center
            owner.gun.lookPoint = owner.target.transform.position + new Vector3(0, 1, 0);
        }

        protected override void Exit()
        {
            owner.TriggerUp();
        }
    }

    // Reload
    public class TurretReloadState : HFSMState<TurretStateComponent>
    {
        public TurretReloadState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Enter()
        {
            owner.Reload();
        }
    }

    /*
     * Turret base FSM leaf/action states
     */
    // Idle
    public class TurretBaseIdleState : HFSMState<TurretStateComponent>
    {
        public TurretBaseIdleState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }
    }

    // Track target
    public class TurretTrackState : HFSMState<TurretStateComponent>
    {
        public TurretTrackState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Execute(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(owner.target.transform.position - owner.baseTransform.position);
            owner.baseTransform.rotation = Quaternion.RotateTowards(owner.baseTransform.rotation, targetRotation, owner.turnSpeed * Time.deltaTime);
        }

    }
}
