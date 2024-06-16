using Gun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerStateComponent : PawnStateComponent
    {
        public InteractableComponent currentInteractable;
        public GunStateComponent equippedGun;
        public GunStateComponent primaryGun;
        public bool isReloading;

        public GameManager gameManager;

        // Start is called before the first frame update
        private void Start()
        {
            EquipGun(primaryGun);
            gameManager.UpdateEquippedGunNameText(equippedGun.gunData.gunName);
        }

        public void EquipGun(GunStateComponent gun)
        {
            equippedGun = gun;
            equippedGun.holder = GetComponent<PlayerInputComponent>();
        }

        // Update is called once per frame
        private void Update()
        {

        }

        public override void Die()
        {
            base.Die();
            GetComponent<PlayerInput>().enabled = false;
            GetComponent<PlayerInputComponent>().enabled = false;
            // TODO: This does not let NPCs un-sense the player
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
