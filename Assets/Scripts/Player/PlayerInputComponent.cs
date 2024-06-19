using System.Collections;
using Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Observer.EventType;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Player
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

        /*
         * Private fields
         */
        private Vector2 moveInput;
        private Vector2 lookInput;

        /*
         * Pre-stored private fields
         */
        private InputActionMap movementActions;
        private InputActionMap combatActions;
        private InputActionMap buildingActions;
        private Vector3 cameraForward, cameraRight;
        private PlayerStateComponent playerStateComponent;

        private void Awake()
        {
            movementActions = inputActionAsset.FindActionMap("Player Movement");
            combatActions = inputActionAsset.FindActionMap("Player Combat Mode");
            buildingActions = inputActionAsset.FindActionMap("Player Build Mode");

            // Bind input functions
            // Move
            movementActions.FindAction("Move").performed += OnMove;
            movementActions.FindAction("Move").canceled += OnMove;
            // Look
            movementActions.FindAction("Look").performed += OnLook;
            // Toggle build mode
            movementActions.FindAction("Toggle Build Mode").performed += ToggleBuildingMode;
            // Interact
            combatActions.FindAction("Interact").performed += OnInteract;
            // Reload
            combatActions.FindAction("Reload").performed += OnReload;
            // Attack
            combatActions.FindAction("Attack").performed += OnStartAttack;
            combatActions.FindAction("Attack").canceled += OnStopAttack;
            // Place
            buildingActions.FindAction("Place").performed += PlaceTurret;
            // Rotate structure
            buildingActions.FindAction("Rotate").performed += RotateTurret;
        }

        private void OnEnable()
        {
            movementActions.Enable();
            combatActions.Enable();
            buildingActions.Disable();
        }

        private void OnDisable()
        {
            movementActions.Disable();
            combatActions.Disable();
            buildingActions.Disable();
        }

        // Start is called before the first frame update
        private void Start()
        {
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

            playerStateComponent = GetComponent<PlayerStateComponent>();
        }

        // Update is called once per frame
        private void Update()
        {
            Move();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void Move()
        {
            if (moveInput == Vector2.zero) {
                return;
            }
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
        }

        // Look at where the mouse is, horizontally
        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
            /*
             * Get mouse position in the 3D world, referenced
             * Code Monkey (2021) 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', Youtube, 23 March
             * https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024)
             */
            Ray ray = followCamera.ScreenPointToRay(lookInput);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Obstacle", "NPC", "Vehicle"))) {
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
                currentInteractable.Interact();
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.isReloading = true;
            // Broadcast event
            GetComponent<SubjectComponent>().NotifyObservers(EventType.IsReloading);
            StartCoroutine(WaitForReloadTime(playerStateComponent.equippedGun.gunData.reloadTime));
        }

        private IEnumerator WaitForReloadTime(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            playerStateComponent.equippedGun.Reload();
            playerStateComponent.isReloading = false;
        }

        private void ToggleBuildingMode(InputAction.CallbackContext context)
        {
            if (!playerStateComponent.isInBuildMode) {
                /*
                 * Currently in combat mode
                 * Enter building mode
                 */
                playerStateComponent.isInBuildMode = true;
                // Instantiate a new turret
                turretToPlace = Instantiate(turretPrefab);
                // Register structure follow function
                movementActions.FindAction("Look").performed += StructureOnCursor;

                // Switch action maps
                combatActions.Disable();
                buildingActions.Enable();
            } else {
                /*
                 * Return to combat mode
                 */
                playerStateComponent.isInBuildMode = false;
                // Destroy unplaced turret
                Destroy(turretToPlace);
                // Un-register structure follow function
                movementActions.FindAction("Look").performed -= StructureOnCursor;

                // Switch action maps
                combatActions.Enable();
                buildingActions.Disable();
            }
        }

        private void StructureOnCursor(InputAction.CallbackContext context)
        {
            turretToPlace.transform.position = lookPoint;
        }

        private void PlaceTurret(InputAction.CallbackContext context)
        {
            turretToPlace = Instantiate(turretPrefab, new Vector3(0, -10, 0), Quaternion.identity);
        }

        private void RotateTurret(InputAction.CallbackContext context)
        {
            // Up and Down are on y axis, 120.0/-120.0
            turretToPlace.transform.Rotate(Vector3.up, -90f * (context.ReadValue<Vector2>().y / 120f));
        }
    }
}
