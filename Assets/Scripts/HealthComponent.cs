using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;

    public void changeHealth(float x)
    {
        // Return if already dead
        if (health == 0) {
            return;
        }

        health += x;
        if (health > maxHealth) {
            health = maxHealth;
        } else if (health < 0) {
            health = 0;
        }

        if (health == 0) {
            GetComponent<PawnStateComponent>().Die();
        }
    }
}
