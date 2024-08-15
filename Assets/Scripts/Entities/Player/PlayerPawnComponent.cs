using Entities.Abilities.Health;
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

            base.Die();
        }
    }
}
