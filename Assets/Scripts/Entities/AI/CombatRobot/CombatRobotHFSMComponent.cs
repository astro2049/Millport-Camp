using Entities.AI.Abilities.HFSM;
using Entities.AI.CombatRobot.States;
using UnityEngine;

namespace Entities.AI.CombatRobot
{
    public class CombatRobotHFSMComponent : MonoBehaviour
    {
        public GunHfsm gunHfsm;
        public MovementHfsm movementHfsm;

        private void Awake()
        {
            CombatRobotStateComponent combatRobotStateComponent = GetComponent<CombatRobotStateComponent>();

            gunHfsm = new GunHfsm(combatRobotStateComponent, HFSMStateType.Branch, "Gun", null);
            movementHfsm = new MovementHfsm(combatRobotStateComponent, HFSMStateType.Branch, "Movement", null);
        }

        private void Update()
        {
            gunHfsm.ExecuteBranch(Time.deltaTime);
            movementHfsm.ExecuteBranch(Time.deltaTime);
        }
    }
}
