using UnityEngine;

namespace Entities.AI.NPC.Zombie
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
            // Debug.Log(other.gameObject.name + " Entered");
            zombieStateComponent.humans.Add(other.gameObject);
            zombieStateComponent.isEngaging = true;
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            // Debug.Log(other.gameObject.name + " Exited");
            zombieStateComponent.humans.Remove(other.gameObject);
            if (zombieStateComponent.humans.Count == 0) {
                zombieStateComponent.isEngaging = false;
            }
        }
    }
}
