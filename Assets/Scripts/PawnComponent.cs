using UnityEngine;

public abstract class PawnComponent : MonoBehaviour
{
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
