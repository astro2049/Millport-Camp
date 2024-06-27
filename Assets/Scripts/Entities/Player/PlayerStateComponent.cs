using Abilities.Interactable;
using Abilities.Observer;
using Entities.Gun;
using EventType = Abilities.Observer.EventType;

namespace Entities.Player
{
    public class PlayerStateComponent : StateComponent
    {
        public InteractableComponent currentInteractable;
        public GunStateComponent equippedGun;
        public GunStateComponent primaryGun;
        public bool isReloading;
        public bool isInBuildMode = false;

        private PlayerObserverComponent playerObserverComponent;
        private SubjectComponent subjectComponent;

        // Start is called before the first frame update
        private void Start()
        {
            playerObserverComponent = GetComponent<PlayerObserverComponent>();
            subjectComponent = GetComponent<SubjectComponent>();

            EquipGun(primaryGun);
        }

        public void EquipGun(GunStateComponent gun)
        {
            equippedGun = gun;

            // Subscribe to equipped gun's events: Reloaded
            equippedGun.GetComponent<SubjectComponent>().AddObserver(playerObserverComponent,
                EventType.Reloaded
            );
            // Tell UI manager about the equipped gun
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.WeaponChanged, equippedGun.gameObject));
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
