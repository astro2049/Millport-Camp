using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;

    public void changeHealth(float x)
    {
        health += x;
        if (health > maxHealth) {
            health = maxHealth;
        } else if (health < 0) {
            health = 0;
        }
    }
}
