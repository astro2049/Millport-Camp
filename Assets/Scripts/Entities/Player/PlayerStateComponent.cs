using Entities.Abilities.Interactable;
using Entities.Abilities.Observer;
using Entities.Abilities.State;
using Entities.Gun;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Player
{
    public enum PlayMode
    {
        Combat = 0,
        Build = 1,
        Inventory = 2
    }

    public class PlayerStateComponent : StateComponent
    {
        public InteractableComponent currentInteractable;
        [SerializeField] private Transform handTransform;
        public GunStateComponent equippedGun;
        public bool isReloading;

        private PlayerObserverComponent playerObserverComponent;
        private SubjectComponent subjectComponent;

        private void Awake()
        {
            playerObserverComponent = GetComponent<PlayerObserverComponent>();
            subjectComponent = GetComponent<SubjectComponent>();
        }

        public void EquipGun(GameObject gun)
        {
            if (equippedGun) {
                UnEquipGun();
            }

            equippedGun = gun.GetComponent<GunStateComponent>();

            // Attach gun to hand
            equippedGun.transform.parent = handTransform;
            equippedGun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Subscribe to equipped gun's events: Reloaded
            equippedGun.GetComponent<SubjectComponent>().AddObserver(playerObserverComponent,
                EventType.Reloaded
            );
            // Tell UI manager about the equipped gun
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.WeaponChanged, equippedGun.gameObject));
        }

        private void UnEquipGun()
        {
            // UnSubscribe to equipped gun's events: Reloaded
            // TODO: More observers to notify?
            equippedGun.GetComponent<SubjectComponent>().RemoveObserver(playerObserverComponent,
                EventType.Reloaded
            );
            equippedGun = null;
        }

        public void OnInteractionStarted(InteractableComponent interactableComponent)
        {
            // Set current interactable
            currentInteractable = interactableComponent;
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.InteractionStarted));
        }

        public void OnInteractionEnded()
        {
            // Clear current interactable
            currentInteractable = null;
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.InteractionEnded));
        }
    }
}
