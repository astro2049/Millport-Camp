using System.Collections;
using UnityEngine;

namespace Entities.Abilities.Health
{
    public class HealthComponent : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;
        public HealthBarComponent healthBarComponent;

        [Header("HP Regeneration")]
        [SerializeField] private bool regen = false;
        [SerializeField] private float regenLag = 3f;
        [SerializeField] private float regenHealthPerTime = 20f;
        [SerializeField] private float regenInterval = 1f;

        private Coroutine regenHealthActivator;
        private Coroutine regenHealthOperator;

        private void Awake()
        {
            health = maxHealth;
        }

        public void ReceiveDamage(float x)
        {
            // Return if already dead
            if (health == 0) {
                return;
            }

            SetHealth(health - x);

            // Trigger death if health is 0
            if (health == 0) {
                GetComponent<ActorComponent>().Die();
            } else if (health < maxHealth && regen) {
                // If HP is not full and regen is on, (re)start HP regeneration
                if (regenHealthActivator != null) {
                    StopCoroutine(regenHealthActivator);
                }
                if (regenHealthOperator != null) {
                    StopCoroutine(regenHealthOperator);
                }
                regenHealthActivator = StartCoroutine(WaitForRegeneration());
            }
        }

        private void RestoreHealth(float x)
        {
            // Return if already dead
            if (health == 0) {
                return;
            }

            SetHealth(health + x);
        }

        private void SetHealth(float x)
        {
            health = Mathf.Clamp(x, 0, maxHealth);
            // Update health bar
            healthBarComponent.greenBar.fillAmount = health / maxHealth;
        }

        private IEnumerator WaitForRegeneration()
        {
            yield return new WaitForSeconds(regenLag);
            regenHealthOperator = StartCoroutine(RegenerateHealth());
        }

        private IEnumerator RegenerateHealth()
        {
            RestoreHealth(regenHealthPerTime);
            while (health < maxHealth) {
                yield return new WaitForSeconds(regenInterval);
                RestoreHealth(regenHealthPerTime);
            }
        }
    }
}
