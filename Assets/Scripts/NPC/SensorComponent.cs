using UnityEngine;

namespace NPC
{
    public class SensorComponent : MonoBehaviour
    {
        private PerceptionComponent perceptionComponent;

        // Start is called before the first frame update
        private void Start()
        {
            perceptionComponent = transform.parent.GetComponent<PerceptionComponent>();
        }

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
