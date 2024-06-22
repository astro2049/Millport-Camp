using AI.HFSM;
using UnityEngine;

namespace AI.Turret.States
{
    /*
     * Turret gun HFSM leaf/action states
     */
    // Idle
    public class TurretTriggerIdleState : HFSMLeafState
    {
        // TODO: There has to be a way to not assign this for every leaf state
        private readonly TurretHFSMComponent turretHfsm;

        public TurretTriggerIdleState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            turretHfsm = owner.GetComponent<TurretHFSMComponent>();
        }

        public override void Execute(float deltaTime)
        {
            if (turretHfsm.TargetIsAligned()) {
                turretHfsm.gunHfsm.triggerFsm.ChangeState("Fire");
            }
        }
    }

    // Fire
    // TODO: Currently the turret only works with AUTO mode guns
    public class TurretFireState : HFSMLeafState
    {
        private readonly TurretStateComponent turret;
        private readonly TurretHFSMComponent turretHfsm;

        public TurretFireState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            turret = owner.GetComponent<TurretStateComponent>();
            turretHfsm = owner.GetComponent<TurretHFSMComponent>();
        }

        public override void Enter()
        {
            turretHfsm.TriggerDown();
        }

        public override void Execute(float deltaTime)
        {
            if (!turretHfsm.TargetIsAligned()) {
                turretHfsm.gunHfsm.triggerFsm.ChangeState("Idle");
                return;
            }
            // Set gun look point to target's chest center
            turret.gun.lookPoint = turretHfsm.target.transform.position + new Vector3(0, 1, 0);
        }

        public override void Exit()
        {
            turretHfsm.TriggerUp();
        }
    }

    // Reload
    public class TurretReloadState : HFSMLeafState
    {
        public TurretReloadState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState) { }

        public override void Enter()
        {
            owner.GetComponent<TurretHFSMComponent>().Reload();
        }
    }

    /*
     * Turret base FSM leaf/action states
     */
    // Idle
    public class TurretBaseIdleState : HFSMLeafState
    {
        public TurretBaseIdleState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState) { }
    }

    // Track target
    public class TurretTrackState : HFSMLeafState
    {
        private readonly TurretStateComponent turret;
        private readonly TurretHFSMComponent turretHfsm;

        public TurretTrackState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            turret = owner.GetComponent<TurretStateComponent>();
            turretHfsm = owner.GetComponent<TurretHFSMComponent>();
        }

        public override void Execute(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(turretHfsm.target.transform.position - turret.baseTransform.position);
            turret.baseTransform.rotation = Quaternion.RotateTowards(turret.baseTransform.rotation, targetRotation, turret.turnSpeed * Time.deltaTime);
        }
    }
}
