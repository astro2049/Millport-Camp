using UnityEngine;

namespace AI.HFSM
{
    /*
     * Hierarchical Finite State Machine (HFSM) leaf state
     * i.e. Action.
     */
    public abstract class HFSMLeafState : HFSMBaseState
    {
        protected HFSMLeafState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState) { }

        public override void Enter()
        {

        }

        public override void Execute(float deltaTime)
        {

        }

        public override void Exit()
        {

        }
    }
}
