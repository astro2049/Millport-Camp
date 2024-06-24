using UnityEngine;

namespace Player
{
    public class PlayerPawnComponent : PawnComponent
    {
        public override void Die()
        {
            // Set rigidbody to kinematic to avoid falling through floor
            GetComponent<Rigidbody>().isKinematic = true;

            base.Die();

            // Disable inputs
            GetComponent<PlayerInputComponent>().enabled = false;
        }
    }
}
