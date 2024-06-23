using Observer;
using Player;
using UnityEngine;
using EventType = Observer.EventType;

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
        GameObject player = other.gameObject;
        // Set player's current interactable to parent
        player.GetComponent<PlayerStateComponent>().currentInteractable = parent.GetComponent<InteractableComponent>();

        // Broadcast event
        player.GetComponent<SubjectComponent>().NotifyObservers(new MCEvent(EventType.InteractionStarted));
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject player = other.gameObject;
        // Clear player's current interactable
        player.GetComponent<PlayerStateComponent>().currentInteractable = null;

        // Broadcast event
        player.GetComponent<SubjectComponent>().NotifyObservers(new MCEvent(EventType.InteractionEnded));
    }
}
