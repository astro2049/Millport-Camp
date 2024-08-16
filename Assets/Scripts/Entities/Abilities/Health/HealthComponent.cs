using UnityEngine;

namespace Entities.Abilities.Health
{
    public class HealthComponent : MonoBehaviour
    {
        public float health = 100f;
        public float maxHealth = 100f;
        public HealthBarComponent healthBarComponent;

        public void changeHealth(float x)
        {
            // Return if already dead
            if (health == 0) {
                return;
            }

            // Update health
            health += x;
            if (health > maxHealth) {
                health = maxHealth;
            } else if (health < 0) {
                health = 0;
            }
            // Update health bar
            healthBarComponent.greenBar.fillAmount = health / maxHealth;

            // Trigger death if health is 0
            if (health == 0) {
                GetComponent<ActorComponent>().Die();
            }
        }
    }
}
