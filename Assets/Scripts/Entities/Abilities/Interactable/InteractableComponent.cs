using UnityEngine;

namespace Entities.Abilities.Interactable
{
    public abstract class InteractableComponent : MonoBehaviour
    {
        public abstract void Interact(GameObject initiator);
    }
}
