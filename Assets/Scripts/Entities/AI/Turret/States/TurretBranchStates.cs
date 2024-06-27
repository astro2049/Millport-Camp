namespace Entities.AI.Turret.States
{
    public class TurretTriggerFsm : HFSMState<TurretStateComponent>
    {
        public TurretTriggerFsm(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            TurretTriggerIdleState idleState = new TurretTriggerIdleState(owner, HFSMStateType.Leaf, "Idle", this);
            TurretFireState fireState = new TurretFireState(owner, HFSMStateType.Leaf, "Fire", this);
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

    public class TurretGunHfsm : HFSMState<TurretStateComponent>
    {
        public readonly TurretTriggerFsm triggerFsm;

        public TurretGunHfsm(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            triggerFsm = new TurretTriggerFsm(owner, HFSMStateType.Branch, "Trigger", this);
            TurretReloadState reloadState = new TurretReloadState(owner, HFSMStateType.Leaf, "Reload", this);
            subStates.Add(triggerFsm.name, triggerFsm);
            subStates.Add(reloadState.name, reloadState);
            current = subStates["Trigger"];
        }
    }

    public class TurretBaseFsm : HFSMState<TurretStateComponent>
    {
        public TurretBaseFsm(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState)
            : base(owner, type, name, parentState)
        {
            TurretBaseIdleState idleState = new TurretBaseIdleState(owner, HFSMStateType.Leaf, "Idle", this);
            TurretTrackState trackState = new TurretTrackState(owner, HFSMStateType.Leaf, "Track", this);
            subStates.Add(idleState.name, idleState);
            subStates.Add(trackState.name, trackState);
            current = subStates["Idle"];
        }
    }
}
