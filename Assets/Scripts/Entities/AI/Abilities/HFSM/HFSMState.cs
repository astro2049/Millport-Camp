﻿using System.Collections.Generic;
using Entities.Abilities.State;

namespace Entities.AI.Abilities.HFSM
{
    /*
     * Hierarchical Finite State Machine (HFSM) state
     * i.e. state state machine - a state, while also being a state machine, having children states
     * Reference (FSM):
     * Gow, J. (2024) 'Agent Behaviour'. ECS7016P: Interactive Agents & Procedural Generation.
     * Available at: https://qmplus.qmul.ac.uk/pluginfile.php/3117853/mod_resource/content/5/Agent-Behaviour.pdf (Accessed: 19 June 2024).
     * Graham, D. (2017) 'A Reusable, Light-Weight Finite-State Machine', in S. Rabin (ed), Game AI Pro 3.
     * Available at: https://www.gameaipro.com/GameAIPro3/GameAIPro3_Chapter12_A_Reusable_Light-Weight_Finite-State_Machine.pdf (Accessed: 19 June 2024).
     * Reference (HFSM):
     * AdamCYounis (2024) 'Code Class - Hierarchical State Machines', YouTube, 18 March.
     * Available at: https://www.youtube.com/watch?v=Z0fmGAQSQG8 (Accessed 27 June 2024).
     */
    public enum HFSMStateType
    {
        Leaf = 0,
        Branch = 1
    }

    public abstract class HFSMState<T> where T : StateComponent
    {
        /*
         * 0. Common fields
         */
        public string name; // Name of the state, e.g. "Idle", "Fire", "Gun"
        private bool isBranch = false;
        protected readonly T owner; // Owner game object's state component, manages context
        protected readonly HFSMState<T> parentState; // Parent branch state i.e. state machine

        /*
         * 1. State Machine fields
         */
        public HFSMState<T> current;
        private readonly Dictionary<string, HFSMState<T>> subStates = new Dictionary<string, HFSMState<T>>();

        private bool requestedToChangeState = false;
        private HFSMState<T> next;

        protected void AddSubStates(params HFSMState<T>[] states)
        {
            foreach (HFSMState<T> state in states) {
                subStates.Add(state.name, state);
            }
            if (subStates.Count > 0) {
                isBranch = true;
            }
        }

        protected HFSMState(T owner, HFSMState<T> parentState)
        {
            this.owner = owner;
            this.parentState = parentState;
        }

        /*
         * 2. State methods
         * These 3 methods are left for derived classes to implement.
         */
        protected virtual void Enter() { }
        protected virtual void Execute(float deltaTime) { }
        protected virtual void Exit() { }

        /*
         * 3. State Machine methods
         * These 4 methods are fixed.
         */
        private void EnterBranch()
        {
            Enter();
            if (isBranch) {
                current.EnterBranch();
            }
        }

        public void ExecuteBranch(float deltaTime)
        {
            Execute(deltaTime);

            // Potential change of active child state
            if (requestedToChangeState) {
                requestedToChangeState = false;
                current.ExitBranch();
                current = next;
                current.EnterBranch();
            }

            if (isBranch) {
                current.ExecuteBranch(deltaTime);
            }
        }

        private void ExitBranch()
        {
            if (isBranch) {
                current.ExitBranch();
            }
            Exit();
        }

        public void ChangeState(string name)
        {
            requestedToChangeState = true;
            next = subStates[name];
        }

        /*
         * 4. Debug methods
         */
        public string GetActiveBranch(List<string> stateNames)
        {
            stateNames.Add(name);

            if (!isBranch) {
                return string.Join(" > ", stateNames);
            }
            return current.GetActiveBranch(stateNames);
        }
    }
}
