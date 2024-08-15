using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.Turret.States
{
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

    public class TurretBaseIdleState : HFSMState<TurretStateComponent>
    {
        public TurretBaseIdleState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }
    }

    public class TurretTrackState : HFSMState<TurretStateComponent>
    {
        public TurretTrackState(TurretStateComponent owner, HFSMStateType type, string name, HFSMState<TurretStateComponent> parentState) : base(owner, type, name, parentState) { }

        protected override void Execute(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(owner.targetTrackerComponent.target.transform.position - owner.baseTransform.position);
            owner.baseTransform.rotation = Quaternion.RotateTowards(owner.baseTransform.rotation, targetRotation, owner.turnSpeed * Time.deltaTime);
        }
    }
}
