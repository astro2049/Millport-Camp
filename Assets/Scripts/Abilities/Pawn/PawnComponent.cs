using Abilities.Health;
using Abilities.Observer;
using UnityEngine;
using EventType = Abilities.Observer.EventType;

namespace Abilities.Pawn
{
    public abstract class PawnComponent : MonoBehaviour
    {
        public virtual void Die()
        {
            // Notify observers of death, if has subject
            // And disable subject
            SubjectComponent subject = GetComponent<SubjectComponent>();
            if (subject) {
                subject.NotifyObservers(new MCEventWEntity(EventType.PawnDead, gameObject));
                subject.enabled = false;
            }

            transform.Rotate(new Vector3(0, 0, 90));
            // Turn off capsule collider
            // TODO: This does not trigger onExit()
            GetComponent<CapsuleCollider>().enabled = false;

            // Disable health bar, if has one
            HealthBarComponent healthBarComponent = GetComponent<HealthComponent>().healthBarComponent;
            if (healthBarComponent) {
                healthBarComponent.enabled = false;
            }
        }
    }
}
