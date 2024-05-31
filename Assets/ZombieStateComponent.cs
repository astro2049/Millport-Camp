using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStateComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GetComponent<MeshFilter>().mesh.bounds);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPerceptionTriggerEnter(Collider other)
    {
        
    }
    
    private void OnPerceptionTriggerExit(Collider other)
    {
        
    }
}
