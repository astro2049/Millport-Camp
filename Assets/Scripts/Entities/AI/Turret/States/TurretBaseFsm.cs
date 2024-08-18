using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.AI.Turret.States
{
    public class TurretBaseFsm : HFSMState<TurretStateComponent>
    {
        public TurretBaseFsm(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState)
            : base(owner, parentState)
        {
            name = "Base";

            TurretBaseIdleState idleState = new TurretBaseIdleState(owner, this);
            TurretTrackState trackState = new TurretTrackState(owner, this);
            AddSubStates(idleState, trackState);
            current = idleState;
        }
    }

    public class TurretBaseIdleState : HFSMState<TurretStateComponent>
    {
        public TurretBaseIdleState(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState) : base(owner, parentState)
        {
            name = "Idle";
        }
    }

    public class TurretTrackState : HFSMState<TurretStateComponent>
    {
        public TurretTrackState(TurretStateComponent owner, HFSMState<TurretStateComponent> parentState) : base(owner, parentState)
        {
            name = "Track";
        }

        protected override void Execute(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(owner.targetTrackerComponent.target.transform.position - owner.baseTransform.position);
            owner.baseTransform.rotation = Quaternion.RotateTowards(owner.baseTransform.rotation, targetRotation, owner.turnSpeed * Time.deltaTime);
        }
    }
}
