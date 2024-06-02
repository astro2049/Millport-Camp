using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SensorComponent : MonoBehaviour
{
    private ZombieStateComponent zombieStateComponent;

    // Start is called before the first frame update
    private void Start()
    {
        zombieStateComponent = transform.parent.GetComponent<ZombieStateComponent>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        zombieStateComponent.OnPerceptionTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        zombieStateComponent.OnPerceptionTriggerExit(other);
    }
}
