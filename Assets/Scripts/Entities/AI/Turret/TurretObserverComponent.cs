using Abilities.Observer;
using EventType = Abilities.Observer.EventType;

namespace Entities.AI.Turret
{
    public class TurretObserverComponent : ObserverComponent
    {
        private TurretStateComponent turretStateComponent;
        private TurretHFSMComponent turretHfsm;

        private void Awake()
        {
            turretStateComponent = GetComponent<TurretStateComponent>();
            turretHfsm = GetComponent<TurretHFSMComponent>();

            // Subscribe to gun events:
            // - AmmoChanged, MagEmpty
            turretStateComponent.gun.GetComponent<SubjectComponent>().AddObserver(this,
                EventType.AmmoChanged,
                EventType.MagEmpty
            );
        }

        public override bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Pawn
                case EventType.PawnDead:
                    turretStateComponent.RemoveTarget((mcEvent as MCEventWEntity)!.entity);
                    break;
                // Gun
                case EventType.AmmoChanged:
                    int ammo = turretStateComponent.gun.magAmmo;
                    turretStateComponent.ammoText.text = ammo.ToString();
                    break;
                case EventType.MagEmpty:
                    turretHfsm.gunHfsm.ChangeState("Reload");
                    break;
                case EventType.Reloaded:
                    turretHfsm.gunHfsm.ChangeState("Trigger");
                    break;
            }
            return true;
        }
    }
}
