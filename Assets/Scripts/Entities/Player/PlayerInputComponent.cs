using System.Collections;
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
        [Header("Movement Settings")] public float moveSpeed = 5f; // 5m/s
        public Camera followCamera;
        public Vector3 lookPoint;
        // Build Mode
        public GameObject turretPrefab;
        private GameObject turretToPlace;
        // Mouse Look at - Layer Masks
        public LayerMask lookAtCombatModeLayers;
        public LayerMask lookAtBuildModeLayers;
        private LayerMask lookAtCurrentLayers;

        /*
         * Private fields
         */
        private Vector2 moveInput;
        private Vector3 velocity;
        private Vector2 lookInput;

        /*
         * Pre-stored private fields
         */
        private InputActionMap movementActions;
        private InputActionMap combatActions;
        private InputActionMap buildActions;
        private Vector3 cameraForward, cameraRight;
        private Rigidbody rb;
        private PlayerStateComponent playerStateComponent;
        private SubjectComponent subjectComponent;

        private void Awake()
        {
            lookAtCurrentLayers = lookAtCombatModeLayers;

            // Get action maps
            movementActions = inputActionAsset.FindActionMap("Player Movement");
            combatActions = inputActionAsset.FindActionMap("Player Combat Mode");
            buildActions = inputActionAsset.FindActionMap("Player Build Mode");

            // Bind input functions
            // Move
            movementActions.FindAction("Move").performed += OnMove;
            movementActions.FindAction("Move").canceled += OnMove;
            // Look
            movementActions.FindAction("Look").performed += OnLook;
            // Toggle build mode
            movementActions.FindAction("Toggle Build Mode").performed += ToggleBuildMode;
            // Interact
            combatActions.FindAction("Interact").performed += OnInteract;
            // Reload
            combatActions.FindAction("Reload").performed += OnReload;
            // Attack
            combatActions.FindAction("Attack").performed += OnStartAttack;
            combatActions.FindAction("Attack").canceled += OnStopAttack;
            // Place
            buildActions.FindAction("Place").performed += PlaceTurret;
            // Rotate structure
            buildActions.FindAction("Rotate").performed += RotateTurret;

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

        // Look at where the mouse is, horizontally
        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
            /*
             * Get mouse position in the 3D world, referenced:
             * Code Monkey (2021) 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', YouTube, 23 March.
             * Available at: https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024).
             */
            Ray ray = followCamera.ScreenPointToRay(lookInput);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, lookAtCurrentLayers)) {
                // Debug: DrawLine - Green if hit NPC, otherwise red
                Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.red);
                lookPoint = hit.point;
                transform.LookAt(new Vector3(
                    lookPoint.x,
                    transform.position.y,
                    lookPoint.z
                ));
                playerStateComponent.equippedGun.lookPoint = lookPoint;
            }
        }

        private void OnStartAttack(InputAction.CallbackContext context)
        {
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.equippedGun.SetIsTriggerDown(true);
        }

        private void OnStopAttack(InputAction.CallbackContext context)
        {
            playerStateComponent.equippedGun.SetIsTriggerDown(false);
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            InteractableComponent currentInteractable = GetComponent<PlayerStateComponent>().currentInteractable;
            if (currentInteractable) {
                currentInteractable.Interact(gameObject);
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.isReloading = true;
            // Broadcast event
            subjectComponent.NotifyObservers(new MCEvent(EventType.IsReloading));
            StartCoroutine(WaitForReloadTime(playerStateComponent.equippedGun.gunData.reloadTime));
        }

        private IEnumerator WaitForReloadTime(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            playerStateComponent.equippedGun.Reload();
            playerStateComponent.isReloading = false;
        }

        private void ToggleBuildMode(InputAction.CallbackContext context)
        {
            if (!playerStateComponent.isInBuildMode) {
                /*
                 * Currently in combat mode
                 * Enter build mode
                 */
                playerStateComponent.isInBuildMode = true;
                // Tell UI manager about the event
                subjectComponent.NotifyObservers(new MCEvent(EventType.EnteredBuildMode));
                // Switch mouse look at layers
                lookAtCurrentLayers = lookAtBuildModeLayers;
                // Instantiate a new turret
                CreateTurret();
                // Register structure follow function
                movementActions.FindAction("Look").performed += StructureOnCursor;

                // Switch action maps
                combatActions.Disable();
                buildActions.Enable();
            } else {
                /*
                 * Return to combat mode
                 */
                playerStateComponent.isInBuildMode = false;
                // Tell UI manager about the event
                subjectComponent.NotifyObservers(new MCEvent(EventType.ExitedBuildMode));
                // Switch mouse look at layers
                lookAtCurrentLayers = lookAtCombatModeLayers;
                // Destroy unplaced turret
                Destroy(turretToPlace);
                // Un-register structure follow function
                movementActions.FindAction("Look").performed -= StructureOnCursor;

                // Switch action maps
                combatActions.Enable();
                buildActions.Disable();
            }
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

        private void PlaceTurret(InputAction.CallbackContext context)
        {
            BuildableComponent buildableComponent = turretToPlace.GetComponent<BuildableComponent>();
            if (buildableComponent.isOkToPlace) {
                buildableComponent.Place();
                // Instantiate a new turret
                CreateTurret();
            }
        }

        private void RotateTurret(InputAction.CallbackContext context)
        {
            // Up and Down are on y axis, 120.0/-120.0
            turretToPlace.transform.Rotate(Vector3.up, -90f * (context.ReadValue<Vector2>().y / 120f));
        }
    }
}
