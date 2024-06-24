using Player;
using UnityEngine;

public class InteractableColliderComponent : MonoBehaviour
{
    private Transform parent;

    // Start is called before the first frame update
    private void Start()
    {
        parent = transform.parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Call player's OnInteractionStarted()
        other.gameObject.GetComponent<PlayerStateComponent>().OnInteractionStarted(parent.GetComponent<InteractableComponent>());
    }

    private void OnTriggerExit(Collider other)
    {
        // Call player's OnInteractionEnded()
        // TODO: Caveat - Probably won't do very well with multiple interactables activated. Possible redesign.
        other.gameObject.GetComponent<PlayerStateComponent>().OnInteractionEnded();
    }
}
