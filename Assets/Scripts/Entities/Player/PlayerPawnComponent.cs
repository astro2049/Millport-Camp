using Abilities.Pawn;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player
{
    public class PlayerPawnComponent : PawnComponent
    {
        public override void Die()
        {
            // Set rigidbody to kinematic to avoid falling through floor
            GetComponent<Rigidbody>().isKinematic = true;

            PlayerInputComponent playerInputComponent = GetComponent<PlayerInputComponent>();
            // Close Inventory
            // TODO: This is a hack, which is just to make sure combat inputs don't get re-enabled, because a disabled component's methods can still execute.
            playerInputComponent.CloseInventory(new InputAction.CallbackContext());
            // Disable inputs
            playerInputComponent.enabled = false;
            GetComponent<PlayerInput>().enabled = false;
            
            base.Die();
        }
    }
}
