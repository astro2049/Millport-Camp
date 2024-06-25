using UnityEngine;

public class BuildableColliderComponent : MonoBehaviour
{
    private BuildableComponent buildableComponent;
    private int overlappingGameObjectsCount = 0;

    private void Awake()
    {
        enabled = false;
    }

    public void init(BuildableComponent buildableComponent)
    {
        this.buildableComponent = buildableComponent;
        enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        overlappingGameObjectsCount++;
        if (overlappingGameObjectsCount == 1) {
            buildableComponent.SetIsOkToPlace(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        overlappingGameObjectsCount--;
        if (overlappingGameObjectsCount == 0) {
            buildableComponent.SetIsOkToPlace(true);
        }
    }
}
