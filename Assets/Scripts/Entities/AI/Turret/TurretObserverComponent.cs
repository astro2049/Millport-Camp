using Observer;

namespace AI.Turret
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
                case EventType.AmmoChanged:
                    int ammo = turretStateComponent.gun.currentMagAmmo;
                    turretStateComponent.ammoText.text = ammo.ToString();
                    break;
                case EventType.MagEmpty:
                    turretHfsm.gunHfsm.ChangeState("Reload");
                    break;
                case EventType.PawnDead:
                    turretStateComponent.RemoveTarget((mcEvent as MCEventWEntity)!.entity);
                    break;
            }
            return true;
        }
    }
}
