using Entities.Abilities.Observer;
using Entities.AI.Abilities.Gunner;
using Entities.AI.Abilities.HFSM;
using Entities.AI.Abilities.TargetTracker;
using Entities.AI.Turret.States;
using TMPro;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Turret
{
    public class TurretHFSMComponent : HFSMComponent, IObserver
    {
        private TargetTrackerComponent targetTrackerComponent;
        private SubjectComponent subjectComponent;
        private GunnerComponent gunnerComponent;

        [SerializeField] private TextMeshPro ammoText;

        // (H)FSMs: Gun, Base
        private TurretGunHfsm gunHfsm;
        private TurretBaseFsm baseFsm;

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
            // Subscribe to gun events:
            gunnerComponent.gun.GetComponent<SubjectComponent>().AddObserver(this,
                EventType.AmmoChanged,
                EventType.MagEmpty,
                EventType.Reloaded
            );

            // Initialize (H)FSMs
            TurretStateComponent turretStateComponent = GetComponent<TurretStateComponent>();
            gunHfsm = new TurretGunHfsm(turretStateComponent, HFSMStateType.Branch, "Gun", null);
            baseFsm = new TurretBaseFsm(turretStateComponent, HFSMStateType.Branch, "Base", null);
        }

        private void Update()
        {
            gunHfsm.ExecuteBranch(Time.deltaTime);
            baseFsm.ExecuteBranch(Time.deltaTime);
        }

        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Target Tracker
                case EventType.AcquiredNewTarget:
                    GameObject enemy = (mcEvent as MCEventWEntity)!.entity;
                    enemy.GetComponent<SubjectComponent>().AddObserver(this, EventType.PawnDead);
                    break;
                case EventType.AcquiredFirstTarget:
                    baseFsm.ChangeState("Track");
                    break;
                case EventType.LostAllTargets:
                    baseFsm.ChangeState("Idle");
                    break;
                // Pawn
                case EventType.PawnDead:
                    GameObject enemy1 = (mcEvent as MCEventWEntity)!.entity;
                    targetTrackerComponent.RemoveTarget(enemy1);
                    break;
                // Gun
                case EventType.AmmoChanged:
                    int ammo = gunnerComponent.gun.magAmmo;
                    ammoText.text = ammo.ToString();
                    break;
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
