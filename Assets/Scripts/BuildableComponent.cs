using System.Collections.Generic;
using UnityEngine;

public class BuildableComponent : MonoBehaviour
{
    public Material buildModeMaterial;
    private readonly Dictionary<MeshRenderer, Material> partMaterials = new Dictionary<MeshRenderer, Material>();

    public void EnterBuildMode()
    {
        //  Assign build mode material
        foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>()) {
            partMaterials.Add(meshRenderer, meshRenderer.material);
            meshRenderer.material = buildModeMaterial;
        }

        // Disable all script components, except self
        foreach (MonoBehaviour component in transform.GetComponents<MonoBehaviour>()) {
            if (component == this) {
                continue;
            }
            component.enabled = false;
        }
    }

    public void Place()
    {
        // Assign back material
        foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>()) {
            meshRenderer.material = partMaterials[meshRenderer];
        }
        partMaterials.Clear();

        // Enable all script components
        foreach (MonoBehaviour component in transform.GetComponents<MonoBehaviour>()) {
            component.enabled = true;
        }
    }
}
