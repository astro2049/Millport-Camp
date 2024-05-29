using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : MonoBehaviour
{
    /*
     * Public fields
     */
    [Header("Movement Settings")] public float moveSpeed = 5f; // 5m/s
    public Transform followCameraTransform;

    /*
     * Private fields
     */
    private Vector2 moveInput;

    /*
     * Precalculated private fields
     */
    private Vector3 cameraForward, cameraRight;

    // Start is called before the first frame update
    private void Start()
    {
        // Calculate camera forward and right vectors
        cameraForward = followCameraTransform.forward;
        cameraRight = followCameraTransform.right;
        // Zero out the y components to stay on the same plane
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        // Normalize the vectors
        cameraForward.Normalize();
        cameraRight.Normalize();
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Move()
    {
        if (moveInput != Vector2.zero) {
            var moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
        }
    }
}
