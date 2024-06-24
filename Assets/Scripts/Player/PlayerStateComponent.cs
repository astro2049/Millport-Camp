using Gun;
using Observer;
using EventType = Observer.EventType;

namespace Player
{
    public class PlayerStateComponent : StateComponent
    {
        public InteractableComponent currentInteractable;
        public GunStateComponent equippedGun;
        public GunStateComponent primaryGun;
        public bool isReloading;
        public bool isInBuildMode = false;

        private SubjectComponent subjectComponent;

        // Start is called before the first frame update
        private void Start()
        {
            subjectComponent = GetComponent<SubjectComponent>();

            EquipGun(primaryGun);
        }

        public void EquipGun(GunStateComponent gun)
        {
            equippedGun = gun;

            // Broadcast event
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.WeaponChanged, equippedGun.gameObject));
        }

        public void OnInteractionStarted(InteractableComponent interactableComponent)
        {
            if (isInBuildMode) {
                return;
            }
            // Set current interactable
            currentInteractable = interactableComponent;
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.InteractionStarted));
        }

        public void OnInteractionEnded()
        {
            if (isInBuildMode) {
                return;
            }
            // Clear current interactable
            currentInteractable = null;
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.InteractionEnded));
        }
    }
}
