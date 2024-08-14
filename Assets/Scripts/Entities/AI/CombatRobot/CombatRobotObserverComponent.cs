using Entities.Abilities.Observer;
using Entities.AI.Abilities.Gunner;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.CombatRobot
{
    public class CombatRobotObserverComponent : ObserverComponent
    {
        private CombatRobotStateComponent combatRobotStateComponent;
        private GunnerComponent gunnerComponent;
        private CombatRobotHFSMComponent combatRobotHfsm;

        private void Awake()
        {
            combatRobotStateComponent = GetComponent<CombatRobotStateComponent>();
            gunnerComponent = GetComponent<GunnerComponent>();
            combatRobotHfsm = GetComponent<CombatRobotHFSMComponent>();

            // Subscribe to gun events:
            // - MagEmpty, Reloaded
            gunnerComponent.gun.GetComponent<SubjectComponent>().AddObserver(this,
                EventType.MagEmpty,
                EventType.Reloaded
            );
        }

        public override bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                // Pawn
                case EventType.PawnDead:
                    combatRobotStateComponent.RemoveTarget((mcEvent as MCEventWEntity)!.entity);
                    // Stop evading, if is evading
                    if (combatRobotHfsm.movementHfsm.combatFsm.current.name == "Evade") {
                        combatRobotHfsm.movementHfsm.combatFsm.ChangeState("Idle");
                    }
                    break;
                // Gun
                case EventType.MagEmpty:
                    combatRobotHfsm.gunHfsm.ChangeState("Reload");
                    break;
                case EventType.Reloaded:
                    combatRobotHfsm.gunHfsm.ChangeState("Trigger");
                    break;
            }
            return true;
        }
    }
}
