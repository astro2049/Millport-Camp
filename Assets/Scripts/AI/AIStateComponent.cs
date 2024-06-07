using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public abstract class AIStateComponent : PawnStateComponent
    {
        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        public abstract void OnPerceptionTriggerEnter(Collider other);

        public abstract void OnPerceptionTriggerExit(Collider other);

        public override void Die()
        {
            base.Die();
            GetComponent<BtComponent>().DeactivateBt();
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
