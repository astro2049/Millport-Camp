using Entities.AI.Abilities.Perception;
using UnityEngine;

namespace Entities.AI.CombatRobot
{
    public class CombatRobotPerceptionComponent : PerceptionComponent
    {
        private CombatRobotStateComponent combatRobotStateComponent;

        private void Awake()
        {
            combatRobotStateComponent = GetComponent<CombatRobotStateComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("CombatRobot")) {
                return;
            }
            combatRobotStateComponent.AddTarget(other.gameObject);
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("CombatRobot")) {
                return;
            }
            combatRobotStateComponent.RemoveTarget(other.gameObject);
        }
    }
}
