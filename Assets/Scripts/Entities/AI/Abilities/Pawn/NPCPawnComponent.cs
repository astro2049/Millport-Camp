using System.Collections;
using Entities.AI.Abilities.Bt;
using Entities.Abilities.Pawn;
using Entities.AI.CombatRobot;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Abilities.Pawn
{
    public class NPCPawnComponent : PawnComponent
    {
        public override void Die()
        {
            base.Die();
            BtComponent btComponent = GetComponent<BtComponent>();

            // Shut down behaviors
            // Zombie
            if (btComponent) {
                btComponent.enabled = false;
            }
            // Combat Robot
            CombatRobotHFSMComponent hfsmComponent = GetComponent<CombatRobotHFSMComponent>();
            if (hfsmComponent) {
                hfsmComponent.enabled = false;
            }

            GetComponent<NavMeshAgent>().enabled = false;
            // StartCoroutine(SinkUnderMap());
        }

        private IEnumerator SinkUnderMap()
        {
            float duration = 5f;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + new Vector3(0, -10, 0);
            float elapsedTime = 0;
            while (elapsedTime < duration) {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
