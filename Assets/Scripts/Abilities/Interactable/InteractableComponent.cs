using UnityEngine;

public abstract class InteractableComponent : MonoBehaviour
{
    public abstract void Interact(GameObject initiator);
}
