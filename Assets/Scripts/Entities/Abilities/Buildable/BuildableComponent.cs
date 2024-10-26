using System.Collections.Generic;
using Entities.Abilities.Observer;
using Unity.VisualScripting;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Abilities.Buildable
{
    public class BuildableComponent : MonoBehaviour
    {
        public Material placeholderMaterial, invalidPositionMaterial;
        private readonly Dictionary<MeshRenderer, Material> partMaterials = new Dictionary<MeshRenderer, Material>();
        // TODO: Caveat - This only works with structures with a single body collider
        public Collider bodyCollider;
        public bool isOkToPlace = true;
        private SubjectComponent subjectComponent;

        private void Awake()
        {
            subjectComponent = GetComponent<SubjectComponent>();
        }

        public void EnterBuildMode()
        {
            // Pre-store mesh renderers and their materials
            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>()) {
                partMaterials.Add(meshRenderer, meshRenderer.material);
            }

            //  Assign build mode material
            ApplyMaterialToParts(placeholderMaterial);
            // Set body's collider to isTrigger and add Build Mode exclusive component BuildableColliderComponent
            bodyCollider.isTrigger = true;
            bodyCollider.AddComponent<BuildableColliderComponent>();
            bodyCollider.GetComponent<BuildableColliderComponent>().init(this);

            // Disable all script components, except self
            foreach (MonoBehaviour component in transform.GetComponents<MonoBehaviour>()) {
                if (component == this) {
                    continue;
                }
                component.enabled = false;
            }
        }

        private void ApplyMaterialToParts(Material material)
        {
            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>()) {
                meshRenderer.material = material;
            }
        }

        public void SetIsOkToPlace(bool yes)
        {
            isOkToPlace = yes;
            if (isOkToPlace) {
                ApplyMaterialToParts(placeholderMaterial);
                subjectComponent.NotifyObservers(new MCEvent(EventType.CanPlace));
            } else {
                ApplyMaterialToParts(invalidPositionMaterial);
                subjectComponent.NotifyObservers(new MCEvent(EventType.CannotPlace));
            }
        }

        public void Place()
        {
            // Assign back material
            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>()) {
                // TODO: Very hacky fix to work with gun->Init() (change of meshes) after EnterBuildMode()
                if (!partMaterials.ContainsKey(meshRenderer)) {
                    continue;
                }
                meshRenderer.material = partMaterials[meshRenderer];
            }
            // Set body's collider back to original and remove Build Mode exclusive component BuildableColliderComponent
            bodyCollider.isTrigger = false;
            Destroy(bodyCollider.transform.GetComponent<BuildableColliderComponent>());

            // Enable all script components
            foreach (MonoBehaviour component in transform.GetComponents<MonoBehaviour>()) {
                component.enabled = true;
            }
        }
    }
}
