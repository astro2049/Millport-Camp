using UnityEngine;

namespace Entities.Abilities.AI.Perception
{
    public class SensorComponent : MonoBehaviour
    {
        private PerceptionComponent perceptionComponent;

        private void Awake()
        {
            perceptionComponent = transform.parent.GetComponent<PerceptionComponent>();
        }

        // Caveat: OnTriggers() might be called before Start(). So do initialize the members in Awake()
        private void OnTriggerEnter(Collider other)
        {
            perceptionComponent.OnPerceptionTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            perceptionComponent.OnPerceptionTriggerExit(other);
        }
    }
}
