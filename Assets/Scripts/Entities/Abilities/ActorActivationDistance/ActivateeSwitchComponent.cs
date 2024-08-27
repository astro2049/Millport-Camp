using UnityEngine;

namespace Entities.Abilities.ActorActivationDistance
{
    public class ActivateeSwitchComponent : MonoBehaviour
    {
        [HideInInspector] public GameObject actor;

        public void Activate()
        {
            actor.SetActive(true);
        }

        public void Deactivate()
        {
            actor.SetActive(false);
        }
    }
}
