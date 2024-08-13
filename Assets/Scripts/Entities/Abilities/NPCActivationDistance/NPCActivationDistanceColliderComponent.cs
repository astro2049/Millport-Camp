using Entities.Abilities.AI.Bt;
using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivationDistanceColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Activate NPC
            other.gameObject.GetComponent<BtComponent>().enabled = true;
        }

        private void OnTriggerExit(Collider other)
        {
            // Deactivate NPC
            other.gameObject.GetComponent<BtComponent>().enabled = false;
        }
    }
}
