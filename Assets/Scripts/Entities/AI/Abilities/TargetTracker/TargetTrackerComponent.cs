using System.Collections.Generic;
using System.Linq;
using Entities.Abilities.Observer;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.AI.Abilities.TargetTracker
{
    public class TargetTrackerComponent : MonoBehaviour, IObserver
    {
        private SubjectComponent subjectComponent;

        public GameObject target; // locked-on enemy
        private readonly HashSet<GameObject> enemies = new HashSet<GameObject>(); // enemy list backlog

        private void Awake()
        {
            subjectComponent = GetComponent<SubjectComponent>();
        }

        public void AddTarget(GameObject enemy)
        {
            // Add enemy to enemies list (set)
            enemies.Add(enemy);
            enemy.GetComponent<SubjectComponent>().AddObserver(this, EventType.Dead);
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.AcquiredNewTarget, enemy));

            // If there's no target now, lock on this enemy
            if (!target) {
                target = enemy;
                subjectComponent.NotifyObservers(new MCEvent(EventType.AcquiredFirstTarget));
            }
        }

        public void RemoveTarget(GameObject enemy)
        {
            // Remove enemy from the enemy list (set)
            enemies.Remove(enemy);

            // Assign new target
            if (enemies.Count != 0) {
                // If the enemy list is not empty, assign first in set as new target
                // TODO: This can be more intelligent, like selecting based on distance
                target = enemies.First();
            } else {
                // If the enemy list is empty, then there's no target at the moment
                target = null;
                subjectComponent.NotifyObservers(new MCEvent(EventType.LostAllTargets));
            }
        }

        public bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                case EventType.Dead:
                    GameObject enemy = (mcEvent as MCEventWEntity)!.entity;
                    RemoveTarget(enemy);
                    break;
            }
            return true;
        }
    }
}
