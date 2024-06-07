using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerStateComponent : PawnStateComponent
    {
        public InteractableComponent currentInteractable;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
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
