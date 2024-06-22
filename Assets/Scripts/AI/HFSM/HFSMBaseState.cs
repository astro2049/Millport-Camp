using UnityEngine;

namespace AI.HFSM
{
    /*
     * Hierarchical Finite State Machine (HFSM) base state, referenced:
     * Gow, J. (2024) 'Agent Behaviour'. ECS7016P: Interactive Agents & Procedural Generation.
     * Available at: https://qmplus.qmul.ac.uk/pluginfile.php/3117853/mod_resource/content/5/Agent-Behaviour.pdf (Accessed: 19 June 2024).
     * Graham, D. (2017) 'A Reusable, Light-Weight Finite-State Machine', in S. Rabin (ed), Game AI Pro 3.
     * Available at: https://www.gameaipro.com/GameAIPro3/GameAIPro3_Chapter12_A_Reusable_Light-Weight_Finite-State_Machine.pdf (Accessed: 19 June 2024).
     */
    public abstract class HFSMBaseState
    {
        public readonly string name;
        protected readonly GameObject owner;
        public readonly HFSMBranchState parentState;

        protected HFSMBaseState(GameObject owner, string name, HFSMBranchState parentState)
        {
            this.name = name;
            this.owner = owner;
            this.parentState = parentState;
        }

        public abstract void Enter();
        public abstract void Execute(float deltaTime);
        public abstract void Exit();
    }
}
