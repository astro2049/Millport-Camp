using Abilities.Buildable;
using Abilities.Interactable;
using Abilities.Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Abilities.Observer.EventType;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Entities.Player
{
    public class PlayerInputComponent : MonoBehaviour
    {
        /*
         * Public fields
         */
        public InputActionAsset inputActionAsset;
        [Header("Movement Settings")]
        public float moveSpeed = 5f; // 5m/s
        public Camera followCamera;
        public Vector3 lookPoint;
        // Build Mode
        public GameObject turretPrefab;
        private GameObject turretToPlace;
        // Mouse Look at - Layer Masks
        public LayerMask lookAtCombatModeLayers;
        public LayerMask lookAtBuildModeLayers;

        /*
         * Private fields
         */
        private Vector2 moveInput;
        private Vector3 velocity;

        /*
         * Pre-stored private fields
         */
        private InputActionMap movementActions;
        private InputActionMap combatActions;
        private InputActionMap buildActions;
        private InputActionMap inventoryActions;
        private Vector3 cameraForward, cameraRight;
        private Rigidbody rb;
        private PlayerStateComponent playerStateComponent;
        private SubjectComponent subjectComponent;

        private void Awake()
        {
            RegisterActionCallbacks();

            /*
             * Precalculate camera forward and right vectors
             */
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

        private void RegisterActionCallbacks()
        {
            /*
             * Movement Actions
             */
            movementActions = inputActionAsset.FindActionMap("Movement");
            // Bind input functions
            // Move
            movementActions.FindAction("Move").performed += OnMove;
            movementActions.FindAction("Move").canceled += OnMove;

            /*
             * Combat Actions
             */
            combatActions = inputActionAsset.FindActionMap("Combat");
            // Look
            combatActions.FindAction("Look").performed += OnCombatLook;
            // Attack
            combatActions.FindAction("Attack").performed += OnStartAttack;
            combatActions.FindAction("Attack").canceled += OnStopAttack;
            // Reload
            combatActions.FindAction("Reload").performed += OnReload;
            // Interact
            combatActions.FindAction("Interact").performed += OnInteract;
            // Build
            combatActions.FindAction("Build").performed += EnterBuildMode;
            // Inventory
            combatActions.FindAction("Inventory").performed += OpenInventory;

            /*
             * Build Actions
             */
            buildActions = inputActionAsset.FindActionMap("Build");
            // Look
            buildActions.FindAction("Look").performed += OnBuildLook;
            buildActions.FindAction("Look").performed += StructureOnCursor;
            // Place
            buildActions.FindAction("Place").performed += PlaceTurret;
            // Rotate structure
            buildActions.FindAction("Rotate").performed += RotateTurret;
            // Quit Build
            buildActions.FindAction("Quit Build").performed += QuitBuildMode;

            /*
             * Inventory
             */
            inventoryActions = inputActionAsset.FindActionMap("Inventory");
            // Look
            inventoryActions.FindAction("Look").performed += OnInventoryLook;
            // Close Inventory
            inventoryActions.FindAction("Close Inventory").performed += CloseInventory;
        }

        private void OnEnable()
        {
            movementActions.Enable();
            combatActions.Enable();
            buildActions.Disable();
        }

        private void OnDisable()
        {
            movementActions.Disable();
            combatActions.Disable();
            buildActions.Disable();
        }

        private void FixedUpdate()
        {
            Move();
        }

        /*
         * Movement Actions
         */
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            velocity = moveDirection * moveSpeed;
        }

        /*
         * Note:
         * We're setting rigidbody's velocity in Move() every physics frame instead of in OnMove(), because it is necessary:
         * - Although rigidbody's drag is 0, we've turned on Use Gravity
         * - And, colliders have a default friction coefficient of 0.6 (see https://docs.unity3d.com/Manual/collider-surface-friction.html)
         * Because of this, the gravity will enforce friction between player and terrain, causing drag.
         * So, if we don't do this, and just assign rigidbody's velocity in OnMove(), the player will move a short distance and quickly stop after a keypress(es), like we're applying impulses.
         * On the other hand, assigning rigidbody's velocity in FixedUpdate() guarantees a constant move speed.
         */
        private void Move()
        {
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
        private void OnCombatLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();
            if (PlayerLookAt(lookInput, lookAtCombatModeLayers)) {
                // TODO: This 'if' is hacky
                if (!playerStateComponent.equippedGun) {
                    return;
                }
                playerStateComponent.equippedGun.lookPoint = lookPoint;
            }
        }

        private void OnStartAttack(InputAction.CallbackContext context)
        {
            // TODO: This 'if' is hacky
            if (!playerStateComponent.equippedGun) {
                return;
            }
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.equippedGun.SetIsTriggerDown(true);
        }

        private void OnStopAttack(InputAction.CallbackContext context)
        {
            // TODO: This 'if' is hacky
            if (!playerStateComponent.equippedGun) {
                return;
            }
            playerStateComponent.equippedGun.SetIsTriggerDown(false);
        }

        private void OnReload(InputAction.CallbackContext context)
        {
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

        private void OnInteract(InputAction.CallbackContext context)
        {
            InteractableComponent currentInteractable = GetComponent<PlayerStateComponent>().currentInteractable;
            if (currentInteractable) {
                currentInteractable.Interact(gameObject);
            }
        }

        private void EnterBuildMode(InputAction.CallbackContext context)
        {
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.EnteredBuildMode));

            // Instantiate a new turret
            CreateTurret();

            // Switch action maps
            combatActions.Disable();
            buildActions.Enable();
        }

        private void OpenInventory(InputAction.CallbackContext context)
        {
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.OpenedInventory));

            // Switch action maps
            combatActions.Disable();
            inventoryActions.Enable();
        }

        /*
         * Build Actions
         */
        // Look at where the mouse is, horizontally
        private void OnBuildLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();
            PlayerLookAt(lookInput, lookAtBuildModeLayers);
        }

        private void StructureOnCursor(InputAction.CallbackContext context)
        {
            turretToPlace.transform.position = lookPoint;
        }

        private void CreateTurret()
        {
            turretToPlace = Instantiate(turretPrefab);
            turretToPlace.GetComponent<BuildableComponent>().EnterBuildMode();
            // Tell UI manager about the turret
            subjectComponent.NotifyObservers(new MCEventWEntity(EventType.PlacingStructure, turretToPlace));
        }

        private void RotateTurret(InputAction.CallbackContext context)
        {
            // Up and Down are on y axis, 120.0/-120.0
            turretToPlace.transform.Rotate(Vector3.up, -90f * (context.ReadValue<Vector2>().y / 120f));
        }

        private void PlaceTurret(InputAction.CallbackContext context)
        {
            BuildableComponent buildableComponent = turretToPlace.GetComponent<BuildableComponent>();
            if (buildableComponent.isOkToPlace) {
                buildableComponent.Place();
                // Instantiate a new turret
                CreateTurret();
            }
        }

        private void QuitBuildMode(InputAction.CallbackContext context)
        {
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.ExitedBuildMode));

            // Destroy unplaced turret
            Destroy(turretToPlace);

            // Switch action maps
            combatActions.Enable();
            buildActions.Disable();
        }

        /*
         * Inventory Actions
         */
        // Look at where the mouse is, horizontally
        private void OnInventoryLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();
            PlayerLookAt(lookInput, lookAtCombatModeLayers);
        }

        public void CloseInventory(InputAction.CallbackContext context)
        {
            // Tell UI manager about the event
            subjectComponent.NotifyObservers(new MCEvent(EventType.ClosedInventory));

            // Switch action maps
            combatActions.Enable();
            inventoryActions.Disable();
        }
    }
}
