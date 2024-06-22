using System.Collections.Generic;
using UnityEngine;

namespace AI.HFSM
{
    /*
     * Hierarchical Finite State Machine (HFSM) branch state
     */
    public abstract class HFSMBranchState : HFSMBaseState
    {
        public HFSMBaseState current;
        protected readonly Dictionary<string, HFSMBaseState> subStates = new Dictionary<string, HFSMBaseState>();

        protected HFSMBranchState(GameObject owner, string name, HFSMBranchState parentState) : base(owner, name, parentState) { }

        public override sealed void Enter()
        {
            current.Enter();
        }

        public override sealed void Execute(float deltaTime)
        {
            current.Execute(deltaTime);
        }

        public override sealed void Exit()
        {
            current.Exit();
        }

        public void ChangeState(string name)
        {
            current.Exit();
            current = subStates[name];
            current.Enter();
        }
    }
}
