using Gun;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Observer.EventType;

namespace Player
{
    public class PlayerStateComponent : PawnStateComponent
    {
        public InteractableComponent currentInteractable;
        public GunStateComponent equippedGun;
        public GunStateComponent primaryGun;
        public bool isReloading;

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
            equippedGun.holder = GetComponent<PlayerInputComponent>();

            // Broadcast event
            subjectComponent.NotifyObservers(EventType.WeaponChanged);
        }

        // Update is called once per frame
        private void Update()
        {

        }

        public override void Die()
        {
            base.Die();
            GetComponent<PlayerInputComponent>().enabled = false;
            // TODO: This does not let NPCs un-sense the player
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
