using Entities.AI.Abilities.HFSM;
using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivationDistanceColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Activate NPC
            HFSMComponent hfsmComponent = other.gameObject.GetComponent<HFSMComponent>();
            if (hfsmComponent) {
                hfsmComponent.enabled = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Deactivate NPC
            HFSMComponent hfsmComponent = other.gameObject.GetComponent<HFSMComponent>();
            if (hfsmComponent) {
                hfsmComponent.enabled = false;
            }
        }
    }
}
