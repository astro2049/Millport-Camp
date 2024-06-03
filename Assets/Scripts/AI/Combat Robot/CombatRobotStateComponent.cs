using System.Collections.Generic;
using UnityEngine;

public class CombatRobotStateComponent : AIStateComponent
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

    public override void OnPerceptionTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name + " Entered");
        humans.Add(other.gameObject);
        isEngaging = true;
    }

    public override void OnPerceptionTriggerExit(Collider other)
    {
        // Debug.Log(other.gameObject.name + " Exited");
        humans.Remove(other.gameObject);
        if (humans.Count == 0) {
            isEngaging = false;
        }
    }
}
