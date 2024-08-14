using Entities.AI.Abilities.Bt;
using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivationDistanceColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Activate NPC
            BtComponent btComponent = other.gameObject.GetComponent<BtComponent>();
            if (btComponent) {
                btComponent.enabled = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Deactivate NPC
            BtComponent btComponent = other.gameObject.GetComponent<BtComponent>();
            if (btComponent) {
                btComponent.enabled = false;
            }
        }
    }
}
