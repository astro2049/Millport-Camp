using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnStateComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Die()
    {
        transform.Rotate(new Vector3(0, 0, 90));
    }
}
