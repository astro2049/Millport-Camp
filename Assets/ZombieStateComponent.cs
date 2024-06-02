using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieStateComponent : PawnStateComponent
{
    public SortedSet<GameObject> humans = new SortedSet<GameObject>();
    public bool isEngaging;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void OnPerceptionTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name + " Entered");
        humans.Add(other.gameObject);
        isEngaging = true;
    }

    public void OnPerceptionTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name + " Exited");
        humans.Remove(other.gameObject);
        if (humans.Count == 0) {
            isEngaging = false;
        }
    }

    public override void Die()
    {
        base.Die();
        GetComponent<ZombieBehaviorTreeComponent>().DeactivateBt();
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
