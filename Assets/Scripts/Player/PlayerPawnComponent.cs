using UnityEngine;

namespace Player
{
    public class PlayerPawnComponent : PawnComponent
    {
        public override void Die()
        {
            base.Die();
            GetComponent<PlayerInputComponent>().enabled = false;
            // TODO: This does not let NPCs un-sense the player
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
