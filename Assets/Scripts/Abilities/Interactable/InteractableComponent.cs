using UnityEngine;

namespace Abilities.Interactable
{
    public abstract class InteractableComponent : MonoBehaviour
    {
        public abstract void Interact(GameObject initiator);
    }
}