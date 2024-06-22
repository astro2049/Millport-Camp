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
            subjectComponent.NotifyObservers(new MCEvent(EventType.WeaponChanged));
        }
    }
}
