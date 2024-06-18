using Observer;
using Player;
using UnityEngine;
using EventType = Observer.EventType;

public class InteractableColliderComponent : MonoBehaviour
{
    private Transform parent;

    private SubjectComponent subjectComponent;

    // Start is called before the first frame update
    private void Start()
    {
        parent = transform.parent;

        subjectComponent = parent.GetComponent<SubjectComponent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Set player's current interactable to parent
        other.gameObject.GetComponent<PlayerStateComponent>().currentInteractable = parent.GetComponent<InteractableComponent>();

        // Broadcast event
        subjectComponent.NotifyObservers(EventType.InteractionStarted);
    }

    private void OnTriggerExit(Collider other)
    {
        // Clear player's current interactable
        other.gameObject.GetComponent<PlayerStateComponent>().currentInteractable = null;

        // Broadcast event
        subjectComponent.NotifyObservers(EventType.InteractionEnded);
    }
}
