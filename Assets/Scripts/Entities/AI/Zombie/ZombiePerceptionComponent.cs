using Entities.AI.Abilities.Perception;
using UnityEngine;

namespace Entities.AI.Zombie
{
    public class ZombiePerceptionComponent : PerceptionComponent
    {
        private ZombieStateComponent zombieStateComponent;

        private void Awake()
        {
            zombieStateComponent = GetComponent<ZombieStateComponent>();
        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Zombie")) {
                return;
            }
            zombieStateComponent.humans.Add(other.gameObject);
            zombieStateComponent.isEngaging = true;
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Zombie")) {
                return;
            }
            zombieStateComponent.humans.Remove(other.gameObject);
            if (zombieStateComponent.humans.Count == 0) {
                zombieStateComponent.isEngaging = false;
            }
        }
    }
}
