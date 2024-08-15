using Entities.Abilities.Observer;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Abilities.Health
{
    public class ActorComponent : MonoBehaviour
    {
        public virtual void Die()
        {
            // If has subject, notify observers of death, and disable self
            SubjectComponent subject = GetComponent<SubjectComponent>();
            if (subject) {
                subject.NotifyObservers(new MCEventWEntity(EventType.Dead, gameObject));
                subject.enabled = false;
            }

            // Disable health bar, if has one
            HealthBarComponent healthBarComponent = GetComponent<HealthComponent>().healthBarComponent;
            if (healthBarComponent) {
                healthBarComponent.enabled = false;
            }
        }
    }
}
