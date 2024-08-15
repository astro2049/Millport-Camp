using Entities.Abilities.Observer;
using Entities.AI.Abilities.HFSM;
using Entities.AI.Abilities.TargetTracker;
using Entities.AI.Zombie.States;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Zombie
{
    public class ZombieHFSMComponent : HFSMComponent, IObserver
    {
        private TargetTrackerComponent targetTrackerComponent;
        private SubjectComponent subjectComponent;

        // HFSMs: Hand, Movement
        public HandHfsm handHfsm;
        private MovementHfsm movementHfsm;

        private void Awake()
        {
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
            subjectComponent = GetComponent<SubjectComponent>();

            // Subscribe to target tracker events
            subjectComponent.AddObserver(this,
                EventType.AcquiredFirstTarget,
                EventType.AcquiredNewTarget,
                EventType.LostAllTargets
            );

            // Initialize HFSMs
            ZombieStateComponent zombieStateComponent = GetComponent<ZombieStateComponent>();
            handHfsm = new HandHfsm(zombieStateComponent, HFSMStateType.Branch, "Hand", null);
            movementHfsm = new MovementHfsm(zombieStateComponent, HFSMStateType.Branch, "Movement", null);
        }

        private void Update()
        {
            handHfsm.ExecuteBranch(Time.deltaTime);
            movementHfsm.ExecuteBranch(Time.deltaTime);
        }

        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Target Tracker
                case EventType.AcquiredNewTarget:
                    GameObject enemy = (mcEvent as MCEventWEntity)!.entity;
                    enemy.GetComponent<SubjectComponent>().AddObserver(this, EventType.Dead);
                    break;
                case EventType.AcquiredFirstTarget:
                    movementHfsm.ChangeState("Chase");
                    break;
                case EventType.LostAllTargets:
                    movementHfsm.ChangeState("Patrol");
                    break;
                // Pawn
                case EventType.Dead:
                    GameObject enemy1 = (mcEvent as MCEventWEntity)!.entity;
                    targetTrackerComponent.RemoveTarget(enemy1);

                    // Stop attacking, if is attacking
                    if (handHfsm.current.name == "Attack") {
                        handHfsm.ChangeState("Idle");
                    }
                    break;
            }
            return true;
        }
    }
}
