using UnityEngine;
using Vehicle;

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
        parent.GetComponent<VehicleStateComponent>().gameManager.ShowInteraction(parent.GetComponent<InteractableComponent>());
    }

    private void OnTriggerExit(Collider other)
    {
        transform.parent.GetComponent<VehicleStateComponent>().gameManager.CloseInteraction();
    }
}
