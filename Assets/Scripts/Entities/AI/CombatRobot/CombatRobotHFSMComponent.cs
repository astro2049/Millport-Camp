using System.Collections.Generic;
using Entities.Abilities.Observer;
using Entities.AI.Abilities.Gunner;
using Entities.AI.Abilities.HFSM;
using Entities.AI.Abilities.TargetTracker;
using Entities.AI.CombatRobot.States;
using TMPro;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.CombatRobot
{
    public class CombatRobotHFSMComponent : HFSMComponent, IObserver
    {
        private TargetTrackerComponent targetTrackerComponent;
        private SubjectComponent subjectComponent;
        private GunnerComponent gunnerComponent;

        // HFSMs: Gun, Movement
        private GunHfsm gunHfsm;
        private MovementHfsm movementHfsm;

        private void Awake()
        {
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
            subjectComponent = GetComponent<SubjectComponent>();
            gunnerComponent = GetComponent<GunnerComponent>();

            // Subscribe to target tracker events
            subjectComponent.AddObserver(this,
                EventType.AcquiredFirstTarget,
                EventType.AcquiredNewTarget,
                EventType.LostAllTargets
            );
            // Subscribe to gun events
            gunnerComponent.gun.GetComponent<SubjectComponent>().AddObserver(this,
                EventType.MagEmpty,
                EventType.Reloaded
            );

            // Initialize HFSMs
            CombatRobotStateComponent combatRobotStateComponent = GetComponent<CombatRobotStateComponent>();
            gunHfsm = new GunHfsm(combatRobotStateComponent, null);
            movementHfsm = new MovementHfsm(combatRobotStateComponent, null);
        }

        [SerializeField] private TextMeshPro hfsmActiveBranchText;

        private void Update()
        {
            gunHfsm.ExecuteBranch(Time.deltaTime);
            movementHfsm.ExecuteBranch(Time.deltaTime);

            hfsmActiveBranchText.text = movementHfsm.GetActiveBranch(new List<string>());
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
                    movementHfsm.ChangeState("Combat");
                    break;
                case EventType.LostAllTargets:
                    movementHfsm.ChangeState("Patrol");
                    break;
                // Pawn
                case EventType.Dead:
                    GameObject enemy1 = (mcEvent as MCEventWEntity)!.entity;
                    // Stop evading from the dead target, if is evading
                    if (enemy1 == targetTrackerComponent.target) {
                        if (movementHfsm.combatFsm.current.name == "Evade") {
                            movementHfsm.combatFsm.ChangeState("Idle");
                        }
                    }
                    break;
                // Gun
                case EventType.MagEmpty:
                    gunHfsm.ChangeState("Reload");
                    break;
                case EventType.Reloaded:
                    gunHfsm.ChangeState("Trigger");
                    break;
            }
            return true;
        }
    }
}
