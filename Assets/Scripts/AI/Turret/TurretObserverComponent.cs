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

            // Subscribe to gun subject
            turretStateComponent.gun.GetComponent<SubjectComponent>().AddObserver(this);
        }

        public override bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                case EventType.AmmoChanged:
                    int ammo = turretStateComponent.gun.currentMagAmmo;
                    turretStateComponent.ammoText.text = ammo.ToString();
                    if (ammo == 0) {
                        turretHfsm.gunHfsm.ChangeState("Reload");
                    }
                    break;
                case EventType.PawnDead:
                    turretHfsm.RemoveTarget((mcEvent as PawnDeadEvent).pawn);
                    break;
            }
            return true;
        }
    }
}
