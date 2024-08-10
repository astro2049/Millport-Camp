using Entities.Abilities.Buildable;
using Entities.Abilities.Input;
using Entities.Abilities.Interactable;
using Entities.Abilities.Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Entities.Abilities.Observer.EventType;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Entities.Player
{
    public class PlayerInputComponent : InputComponent
    {
        /*
         * Public fields
         */
        [Header("Movement Settings")]
        public float moveSpeed = 5f; // 5m/s
        public Vector3 lookPoint;
        // Build Mode
        public GameObject turretPrefab;
        private GameObject turretToPlace;
        // Mouse Look at - Layer Masks
        public LayerMask lookAtCombatModeLayers;
        public LayerMask lookAtBuildModeLayers;

        // movement
        private Vector2 moveInput;
        private Vector3 velocity;

        /*
         * Pre-stored private fields
         */
        private Camera followCamera;
        private InputActionMap movementActions, combatActions, buildActions;
        private Vector3 cameraForward, cameraRight;
        private Rigidbody rb;
        private PlayerStateComponent playerStateComponent;
        private SubjectComponent subjectComponent;
        private PlayerInput playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            movementActions = playerInput.actions.FindActionMap("Movement");
            combatActions = playerInput.actions.FindActionMap("Combat");
            buildActions = playerInput.actions.FindActionMap("Build");

            /*
             * Precalculate camera forward and right vectors
             */
            followCamera = Camera.main;
            cameraForward = followCamera.transform.forward;
            cameraRight = followCamera.transform.right;
            // Zero out the y components to stay on the same plane
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            // Normalize the vectors
            cameraForward.Normalize();
            cameraRight.Normalize();

            rb = GetComponent<Rigidbody>();
            // Components
            playerStateComponent = GetComponent<PlayerStateComponent>();
            subjectComponent = GetComponent<SubjectComponent>();
        }

        private void OnEnable()
        {
            movementActions.Enable();
            combatActions.Enable();
        }

        private void OnDisable()
        {
            playerInput.actions.Disable();
        }

        private void FixedUpdate()
        {
            Move();
        }

        /*
         * Movement Actions
         */
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled) {
                return;
            }

            moveInput = context.ReadValue<Vector2>();
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            velocity = moveDirection * moveSpeed;
        }

        private void Move()
        {
            /*
             * Note:
             * We're setting rigidbody's velocity in Move() every physics frame instead of in OnMove(), because it is necessary:
             * - Although rigidbody's drag is 0, we've turned on Use Gravity
             * - And, colliders have a default friction coefficient of 0.6 (see https://docs.unity3d.com/Manual/collider-surface-friction.html)
             * Because of this, the gravity will enforce friction between player and terrain, causing drag.
             * So, if we don't do this, and just assign rigidbody's velocity in OnMove(), the player will move a short distance and quickly stop after a keypress(es), like we're applying impulses.
             * On the other hand, assigning rigidbody's velocity in FixedUpdate() guarantees a constant move speed.
             */
            rb.velocity = velocity;
        }

        private bool PlayerLookAt(Vector2 lookInput, LayerMask lookAtLayers)
        {
            /*
             * Get mouse position in the 3D world, referenced:
             * Code Monkey (2021) 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', YouTube, 23 March.
             * Available at: https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024).
             */
            Ray ray = followCamera.ScreenPointToRay(lookInput);
            bool hasHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, lookAtLayers);
            if (!hasHit) {
                return false;
            }
            // Debug: DrawLine - Green if hit NPC, otherwise red
            Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.red);
            lookPoint = hit.point;
            transform.LookAt(new Vector3(
                lookPoint.x,
                transform.position.y,
                lookPoint.z
            ));
            return true;
        }

        /*
         * Combat Actions
         */
        // Look at where the mouse is, horizontally
        public void OnCombatLook(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            Vector2 lookInput = context.ReadValue<Vector2>();
            if (PlayerLookAt(lookInput, lookAtCombatModeLayers)) {
                // TODO: This 'if' is hacky
                if (!playerStateComponent.equippedGun) {
                    return;
                }
                playerStateComponent.equippedGun.lookPoint = lookPoint;
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                // TODO: This 'if' is hacky
                if (!playerStateComponent.equippedGun) {
                    return;
                }
                if (playerStateComponent.isReloading) {
                    return;
                }
                playerStateComponent.equippedGun.SetIsTriggerDown(true);
            } else if (context.phase == InputActionPhase.Canceled) {
                // TODO: This 'if' is hacky
                if (!playerStateComponent.equippedGun) {
                    return;
                }
                playerStateComponent.equippedGun.SetIsTriggerDown(false);
            }
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            // TODO: This 'if' is hacky
            if (!playerStateComponent.equippedGun) {
                return;
            }
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.isReloading = true;
            // Broadcast event
            subjectComponent.NotifyObservers(new MCEvent(EventType.IsReloading));
            StartCoroutine(playerStateComponent.equippedGun.StartReloading());
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            InteractableComponent currentInteractable = GetComponent<PlayerStateComponent>().currentInteractable;
            if (currentInteractable) {
                currentInteractable.Interact(gameObject);
            }
        }

        public void EnterBuildMode(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.EnteredBuildMode));

            // Instantiate a new turret
            CreateTurret();

            // Switch action maps
            combatActions.Disable();
            buildActions.Enable();
        }

        /*
         * Build Actions
         */
        // Look at where the mouse is, horizontally
        public void OnBuildLook(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            Vector2 lookInput = context.ReadValue<Vector2>();
            PlayerLookAt(lookInput, lookAtBuildModeLayers);
        }

        public void StructureOnCursor(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            turretToPlace.transform.position = lookPoint;
        }

        public void CreateTurret()
        {
            turretToPlace = Instantiate(turretPrefab);
            turretToPlace.GetComponent<BuildableComponent>().EnterBuildMode();
            // Tell UI manager about the turret
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.PlacingStructure, turretToPlace));
        }

        public void RotateTurret(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            // Up and Down are on y axis, 120.0/-120.0
            turretToPlace.transform.Rotate(Vector3.up, -90f * (context.ReadValue<Vector2>().y / 120f));
        }

        public void PlaceTurret(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            BuildableComponent buildableComponent = turretToPlace.GetComponent<BuildableComponent>();
            if (buildableComponent.isOkToPlace) {
                buildableComponent.Place();
                // Instantiate a new turret
                CreateTurret();
            }
        }

        public void QuitBuildMode(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.ExitedBuildMode));

            // Destroy unplaced turret
            Destroy(turretToPlace);

            // Switch action maps
            combatActions.Enable();
            buildActions.Disable();
        }
    }
}
