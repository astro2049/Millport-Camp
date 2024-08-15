using UnityEngine;

namespace Entities.Abilities.Health
{
    public abstract class PawnComponent : ActorComponent
    {
        public override void Die()
        {
            base.Die();

            // Lie on the ground
            transform.Rotate(new Vector3(0, 0, 90));

            // Turn off capsule collider
            // TODO: This does not trigger onExit()
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
