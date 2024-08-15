using Entities.Abilities.Observer;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Abilities.Perception
{
    public class NPCPerceptionComponent : PerceptionComponent
    {
        private TargetTrackerComponent targetTrackerComponent;

        private void Awake()
        {
            // Configure perception
            CreateSensorCollider();

            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            GameObject actor = other.gameObject;
            bool isPlayerControlled = false;

            if (!IsInterested(actor, ref isPlayerControlled)) {
                return;
            }

            targetTrackerComponent.AddTarget(actor);
            if (isPlayerControlled) {
                actor.GetComponent<SubjectComponent>().AddObserver(targetTrackerComponent, EventType.NotControlledByPlayer);
            }
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            GameObject actor = other.gameObject;
            bool isPlayerControlled = false;

            if (!IsInterested(actor, ref isPlayerControlled)) {
                return;
            }

            targetTrackerComponent.RemoveTarget(actor);
            if (isPlayerControlled) {
                actor.GetComponent<SubjectComponent>().RemoveObserver(targetTrackerComponent, EventType.NotControlledByPlayer);
            }
        }

        private bool IsInterested(GameObject actor, ref bool isPlayerControlled)
        {
            int layer = actor.layer;
            if (layer == LayerMask.NameToLayer("NPC")) {
                // Ignore own kind
                if (actor.CompareTag(gameObject.tag)) {
                    return false;
                }
                return true;
            } else {
                // Only interested in player
                if (actor.CompareTag("Player")) {
                    isPlayerControlled = true;
                    return true;
                }
                return false;
            }
        }
    }
}
