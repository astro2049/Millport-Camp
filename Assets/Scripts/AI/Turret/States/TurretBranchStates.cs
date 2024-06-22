using AI.HFSM;
using UnityEngine;

namespace AI.Turret.States
{
    public class TurretTriggerFsm : HFSMBranchState
    {
        public TurretTriggerFsm(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            TurretTriggerIdleState idleState = new TurretTriggerIdleState(owner, "Idle", this);
            TurretFireState fireState = new TurretFireState(owner, "Fire", this);
            subStates.Add(idleState.name, idleState);
            subStates.Add(fireState.name, fireState);
            current = subStates["Idle"];
        }
    }

    public class TurretGunHfsm : HFSMBranchState
    {
        public readonly TurretTriggerFsm triggerFsm;
        
        public TurretGunHfsm(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            triggerFsm = new TurretTriggerFsm(owner, "Trigger", this);
            TurretReloadState reloadState = new TurretReloadState(owner, "Reload", this);
            subStates.Add(triggerFsm.name, triggerFsm);
            subStates.Add(reloadState.name, reloadState);
            current = subStates["Trigger"];
        }
    }

    public class TurretBaseFsm : HFSMBranchState
    {
        public TurretBaseFsm(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState)
        {
            TurretBaseIdleState idleState = new TurretBaseIdleState(owner, "Idle", this);
            TurretTrackState trackState = new TurretTrackState(owner, "Track", this);
            subStates.Add(idleState.name, idleState);
            subStates.Add(trackState.name, trackState);
            current = subStates["Idle"];
        }
    }
}
