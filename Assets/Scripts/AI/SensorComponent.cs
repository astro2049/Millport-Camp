using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorComponent : MonoBehaviour
{
    private AIStateComponent aiStateComponent;

    // Start is called before the first frame update
    private void Start()
    {
        aiStateComponent = transform.parent.GetComponent<AIStateComponent>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        aiStateComponent.OnPerceptionTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        aiStateComponent.OnPerceptionTriggerExit(other);
    }
}
