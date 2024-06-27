using Entities;
using UnityEngine;

namespace Abilities.Health
{
    public class HealthComponent : MonoBehaviour
    {
        public float health = 100f;
        public float maxHealth = 100f;
        [HideInInspector] public HealthBarComponent healthBarComponent;

        // Start is called before the first frame update
        private void Start()
        {
            Transform healthBarTransform = transform.Find("Prefab_Health_Bar");
            if (healthBarTransform) {
                healthBarComponent = healthBarTransform.GetComponent<HealthBarComponent>();
            }
        }

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
            if (healthBarComponent) {
                healthBarComponent.greenBar.fillAmount = health / maxHealth;
            }

            // Trigger death if health is 0
            if (health == 0) {
                GetComponent<PawnComponent>().Die();
            }
        }
    }
}
