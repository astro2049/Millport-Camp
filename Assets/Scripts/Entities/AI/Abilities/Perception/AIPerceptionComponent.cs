using System.Linq;
using Entities.Abilities.Observer;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Abilities.Perception
{
    public class AIPerceptionComponent : PerceptionComponent
    {
        [SerializeField] private string[] tags;

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
            if (!tags.Contains(actor.tag)) {
                return;
            }

            targetTrackerComponent.AddTarget(actor);
            if (actor.CompareTag("Player")) {
                actor.GetComponent<SubjectComponent>().AddObserver(targetTrackerComponent, EventType.NotControlledByPlayer);
            }
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            GameObject actor = other.gameObject;
            if (!tags.Contains(actor.tag)) {
                return;
            }

            targetTrackerComponent.RemoveTarget(actor);
            if (actor.CompareTag("Player")) {
                actor.GetComponent<SubjectComponent>().RemoveObserver(targetTrackerComponent, EventType.NotControlledByPlayer);
            }
        }

        private void OnDestroy()
        {
            Destroy(sensorGameObject);
        }
    }
}
