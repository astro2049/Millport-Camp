using System;
using Entities.Abilities.Buildable;
using Entities.Abilities.Input;
using Entities.Abilities.Interactable;
using Entities.Abilities.Observer;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Entities.Abilities.Observer.EventType;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Entities.Player
{
    public class PlayerInputComponent : InputComponent
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f; // 5m/s
        [SerializeField] private Vector3 lookPoint;
        [Header("Build Mode")]
        [SerializeField] private GameObject turretPrefab;
        private GameObject turretToPlace;
        [Header("Mouse Look At")]
        [SerializeField] private LayerMask lookAtCombatModeLayers;
        [SerializeField] private LayerMask lookAtBuildModeLayers;

        // movement
        private Vector2 moveInput;
        private Vector3 velocity;

        // Components
        private Camera followCamera;
        private Vector3 cameraForward, cameraRight;
        private Rigidbody rb;
        private PlayerStateComponent playerStateComponent;
        private SubjectComponent subjectComponent;

        // Action maps
        private InputActionMap movementActions, combatActions, buildActions;

        private void Awake()
        {
            // Get action maps
            movementActions = InputManager.instance.gameplayActionMaps.FindActionMap("Movement");
            combatActions = InputManager.instance.gameplayActionMaps.FindActionMap("Combat");
            buildActions = InputManager.instance.gameplayActionMaps.FindActionMap("Build");

            // Precalculate camera forward and right vectors
            followCamera = Camera.main;
            cameraForward = followCamera.transform.forward;
            cameraRight = followCamera.transform.right;
            // Zero out the y components to stay on the same plane
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            // Normalize the vectors
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Get components
            rb = GetComponent<Rigidbody>();
            playerStateComponent = GetComponent<PlayerStateComponent>();
            subjectComponent = GetComponent<SubjectComponent>();
        }

        private void OnEnable()
        {
            // Bind actions to inputs
            BindInputActions();

            movementActions.Enable();
            combatActions.Enable();
            // buildActions.Enable();
        }

        private void OnDisable()
        {
            // UnBind actions to inputs
            UnBindInputActions();

            movementActions.Disable();
            combatActions.Disable();
            buildActions.Disable();
        }

        private void BindInputActions()
        {
            movementActions.FindAction("Move").performed += OnMove;
            movementActions.FindAction("Move").canceled += OnMove;

            combatActions.FindAction("Look").performed += OnCombatLook;
            combatActions.FindAction("Attack").performed += OnStartAttack;
            combatActions.FindAction("Attack").canceled += OnStopAttack;
            combatActions.FindAction("Reload").performed += OnReload;
            combatActions.FindAction("Interact").performed += OnInteract;
            combatActions.FindAction("Build").performed += EnterBuildMode;

            buildActions.FindAction("Look").performed += OnBuildLook;
            buildActions.FindAction("Look").performed += StructureOnCursor;
            buildActions.FindAction("Place").performed += PlaceTurret;
            buildActions.FindAction("Rotate").performed += RotateTurret;
            buildActions.FindAction("Quit Build").performed += QuitBuildMode;
        }

        private void UnBindInputActions()
        {
            movementActions.FindAction("Move").performed -= OnMove;
            movementActions.FindAction("Move").canceled -= OnMove;

            combatActions.FindAction("Look").performed -= OnCombatLook;
            combatActions.FindAction("Attack").performed -= OnStartAttack;
            combatActions.FindAction("Attack").canceled -= OnStopAttack;
            combatActions.FindAction("Reload").performed -= OnReload;
            combatActions.FindAction("Interact").performed -= OnInteract;
            combatActions.FindAction("Build").performed -= EnterBuildMode;

            buildActions.FindAction("Look").performed -= OnBuildLook;
            buildActions.FindAction("Look").performed -= StructureOnCursor;
            buildActions.FindAction("Place").performed -= PlaceTurret;
            buildActions.FindAction("Rotate").performed -= RotateTurret;
            buildActions.FindAction("Quit Build").performed -= QuitBuildMode;
        }

        private void FixedUpdate()
        {
            Move();
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

        /*
         * Movement Actions
         */
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            velocity = moveDirection * moveSpeed;
        }

        private bool LookAt(Vector2 lookInput, LayerMask lookAtLayers)
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
            if (LookAt(lookInput, lookAtCombatModeLayers)) {
                // TODO: This 'if' is hacky
                if (!playerStateComponent.equippedGun) {
                    return;
                }
                playerStateComponent.equippedGun.transform.rotation = Quaternion.LookRotation(lookPoint - playerStateComponent.equippedGun.transform.position);
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

        /*
         * Build Actions
         */
        // Look at where the mouse is, horizontally
        private void OnBuildLook(InputAction.CallbackContext context)
        {
            Vector2 lookInput = context.ReadValue<Vector2>();
            LookAt(lookInput, lookAtBuildModeLayers);
        }

        private void StructureOnCursor(InputAction.CallbackContext context)
        {
            turretToPlace.transform.position = lookPoint;
        }

        private void CreateTurret()
        {
            turretToPlace = Instantiate(turretPrefab, lookPoint, Quaternion.identity);
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
            // TODO: Destroying parent is hacky
            Destroy(turretToPlace.transform.parent.gameObject);

            // Switch action maps
            combatActions.Enable();
            buildActions.Disable();
        }
    }
}
