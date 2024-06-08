using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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
        public Vector3 hitPoint;

        /*
         * Private fields
         */
        private Vector2 moveInput;
        private Vector2 lookInput;

        /*
         * Pre-stored private fields
         */
        private Vector3 cameraForward, cameraRight;
        private PlayerStateComponent playerStateComponent;

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

            InputAction attackInputAction = inputActionAsset.FindActionMap("Player").FindAction("Attack");
            attackInputAction.performed += OnStartAttack;
            attackInputAction.canceled += OnStopAttack;
        }

        // Update is called once per frame
        private void Update()
        {
            Move();
            Look();
        }

        private void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void Move()
        {
            if (moveInput == Vector2.zero) {
                return;
            }
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
        }

        private void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        // Look at where the mouse is, horizontally
        private void Look()
        {
            /*
             * Get mouse position in the 3D world, referenced
             * Code Monkey (2021) 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', Youtube, 23 March
             * https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024)
             */
            Ray ray = followCamera.ScreenPointToRay(lookInput);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Obstacle", "NPC", "Vehicle"))) {
                // Debug: DrawLine - Green if hit NPC, otherwise red
                Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.red);
                hitPoint = hit.point;
                transform.LookAt(new Vector3(
                    hitPoint.x,
                    transform.position.y,
                    hitPoint.z
                ));
            }
        }

        private void OnStartAttack(InputAction.CallbackContext context)
        {
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.equippedGun.SetIsTriggerDown(true);
        }

        public Vector3 LookAtPoint()
        {
            return hitPoint;
        }

        private void OnStopAttack(InputAction.CallbackContext context)
        {
            playerStateComponent.equippedGun.SetIsTriggerDown(false);
        }

        private void OnInteract()
        {
            InteractableComponent currentInteractable = GetComponent<PlayerStateComponent>().currentInteractable;
            if (currentInteractable) {
                currentInteractable.Interact();
            }
        }

        private void OnReload()
        {
            if (playerStateComponent.isReloading) {
                return;
            }
            playerStateComponent.isReloading = true;
            StartCoroutine(WaitForReloadTime(playerStateComponent.equippedGun.reloadTime));
        }

        private IEnumerator WaitForReloadTime(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            playerStateComponent.equippedGun.Reload();
            playerStateComponent.isReloading = false;
        }
    }
}
