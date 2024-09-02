using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActivateeSwitchComponent : MonoBehaviour
    {
        [HideInInspector] public GameObject actor;

        public void Activate()
        {
            actor.SetActive(true);

            // Attach self to actor
            transform.parent = actor.transform;
        }

        public void Deactivate()
        {
            // Attach self to (actor's) parent
            transform.parent = actor.transform.parent;

            actor.SetActive(false);
        }
    }
}
