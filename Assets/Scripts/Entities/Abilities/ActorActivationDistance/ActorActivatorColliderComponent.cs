using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActorActivatorColliderComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Ignore self collision
            // TODO: hacky
            if (other.transform.parent == transform.parent.parent) {
                return;
            }

            // Activate NPC
            other.transform.parent.GetComponent<ActivateeComponent>().Activate();
        }

        private void OnTriggerExit(Collider other)
        {
            // Ignore self collision
            // TODO: hacky
            if (other.transform.parent == transform.parent.parent) {
                return;
            }

            // Deactivate NPC
            other.transform.parent.GetComponent<ActivateeComponent>().Deactivate();
        }
    }
}
