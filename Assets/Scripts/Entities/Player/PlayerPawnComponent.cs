using Entities.Abilities.Pawn;
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

            // Disable inputs
            PlayerInputComponent playerInputComponent = GetComponent<PlayerInputComponent>();
            playerInputComponent.enabled = false;
            GetComponent<PlayerInput>().enabled = false;

            base.Die();
        }
    }
}
