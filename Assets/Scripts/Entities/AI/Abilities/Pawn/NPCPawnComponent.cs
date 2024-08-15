using System.Collections;
using Entities.Abilities.Pawn;
using Entities.AI.Abilities.HFSM;
using Entities.AI.Abilities.Perception;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Abilities.Pawn
{
    public class NPCPawnComponent : PawnComponent
    {
        public override void Die()
        {
            base.Die();

            // Shut down perception, behaviors and navmesh agent
            GetComponent<PerceptionComponent>().enabled = false;
            GetComponent<HFSMComponent>().enabled = false;
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
