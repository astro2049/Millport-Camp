using System.Collections.Generic;
using Entities.Abilities.Observer;
using Entities.AI.Abilities.HFSM;
using Entities.AI.Zombie.States;
using TMPro;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Zombie
{
    public class ZombieHFSMComponent : HFSMComponent, IObserver
    {
        private SubjectComponent subjectComponent;

        // HFSMs: Hand, Movement
        public HandHfsm handHfsm;
        private MovementHfsm movementHfsm;

        private void Awake()
        {
            subjectComponent = GetComponent<SubjectComponent>();

            // Subscribe to target tracker events
            subjectComponent.AddObserver(this,
                EventType.AcquiredFirstTarget,
                EventType.AcquiredNewTarget,
                EventType.LostAllTargets
            );

            // Initialize HFSMs
            ZombieStateComponent zombieStateComponent = GetComponent<ZombieStateComponent>();
            handHfsm = new HandHfsm(zombieStateComponent, null);
            movementHfsm = new MovementHfsm(zombieStateComponent, null);
        }

        [SerializeField] private TextMeshPro hfsmActiveBranchText;

        private void Update()
        {
            handHfsm.ExecuteBranch(Time.deltaTime);
            movementHfsm.ExecuteBranch(Time.deltaTime);

            hfsmActiveBranchText.text = handHfsm.GetActiveBranch(new List<string>());
        }

        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Target Tracker
                case EventType.AcquiredFirstTarget:
                    movementHfsm.ChangeState("Chase");
                    break;
                case EventType.LostAllTargets:
                    handHfsm.ChangeState("Idle");
                    movementHfsm.ChangeState("Patrol");
                    break;
            }
            return true;
        }
    }
}
