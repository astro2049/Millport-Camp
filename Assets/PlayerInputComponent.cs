using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : MonoBehaviour
{
    /*
     * Public fields
     */
    [Header("Movement Settings")] public float moveSpeed = 5f; // 5m/s
    public Camera followCamera;

    /*
     * Private fields
     */
    private Vector2 moveInput;
    private GameObject gameObjectHit;

    /*
     * Precalculated private fields
     */
    private Vector3 cameraForward, cameraRight;

    // Start is called before the first frame update
    private void Start()
    {
        // Precalculate camera forward and right vectors
        cameraForward = followCamera.transform.forward;
        cameraRight = followCamera.transform.right;
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
        Look();
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Move()
    {
        if (moveInput != Vector2.zero) {
            Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;
            transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
        }
    }

    private void Look()
    {
        /*
         * Code Monkey (2021), 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', Youtube, 23 March
         * https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024)
         */
        Ray ray = followCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "NPC"))) {
            gameObjectHit = hit.collider.gameObject;
            if (gameObjectHit.layer == LayerMask.NameToLayer("Terrain")) {
                Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, Color.red);
            } else {
                Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, Color.green);
            }
            transform.LookAt(new Vector3(
                hit.point.x,
                transform.position.y,
                hit.point.z
            ));
        } else {
            gameObjectHit = null;
        }
    }

    private void OnAttack()
    {
        if (gameObjectHit && gameObjectHit.layer == LayerMask.NameToLayer("NPC")) {
            Debug.Log(gameObjectHit.name);
            GetComponent<DamageDealerComponent>().DealDamage(gameObjectHit, 100f);
        }
    }
}
