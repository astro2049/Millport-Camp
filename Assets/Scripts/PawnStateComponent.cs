using UnityEngine;

public abstract class PawnStateComponent : MonoBehaviour
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
        
        // Disable health bar, if has one
        HealthBarComponent healthBarComponent = GetComponent<HealthComponent>().healthBarComponent;
        if (healthBarComponent) {
            healthBarComponent.enabled = false;
        }
    }
}
