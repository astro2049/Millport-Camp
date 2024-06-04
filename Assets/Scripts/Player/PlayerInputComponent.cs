using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : MonoBehaviour
{
    /*
     * Public fields
     */
    [Header("Movement Settings")] public float moveSpeed = 5f; // 5m/s
    public Camera followCamera;
    // Tracer
    public GameObject tracerPrefab;
    public float tracerSpeed = 150f; // 150m/s

    /*
     * Private fields
     */
    private Vector2 moveInput;
    private Transform muzzleTransform;

    /*
     * Precalculated private fields
     */
    private Vector3 cameraForward, cameraRight;

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

        /*
         * Precalculate muzzle transform
         */
        muzzleTransform = transform.Find("Muzzle");
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

    private void Look()
    {
        /*
         * Code Monkey (2021), 'How to get Mouse Position in 3D and 2D! (Unity Tutorial)', Youtube, 23 March
         * https://www.youtube.com/watch?v=0jTPKz3ga4w (Accessed 30 May 2024)
         */
        Ray ray = followCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Obstacle", "NPC"))) {
            // Debug: DrawLine - Green if hit NPC, otherwise red
            Debug.DrawLine(followCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()), hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.red);
            transform.LookAt(new Vector3(
                hit.point.x,
                transform.position.y,
                hit.point.z
            ));
        }
    }

    private void OnAttack()
    {
        Vector3 muzzlePosition = muzzleTransform.position;
        Ray ray = new Ray(muzzlePosition, transform.forward);
        TrailRenderer tracerTrail = Instantiate(tracerPrefab, muzzlePosition, Quaternion.LookRotation(transform.forward)).GetComponent<TrailRenderer>();
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Obstacle", "NPC"))) {
            // Debug: DrawLine - Green if hit NPC, otherwise cyan
            Debug.DrawLine(muzzlePosition, hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.cyan, 5f);
            // Tracer effect
            StartCoroutine(SpawnTrail(tracerTrail, hit.distance));
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC")) {
                // Deal damage
                GetComponent<DamageDealerComponent>().DealDamage(hit.collider.gameObject, 100f);
            }
        } else {
            // Tracer effect, endPoint is arbitrary (100m away, out of screen)
            StartCoroutine(SpawnTrail(tracerTrail, 100f));
        }
    }

    /*
     * Tracer Effect
     * TheKiwiCoder (2020), '[#05] Shooting a weapon using Projectile Raycasts (with effects)', Youtube, 17 May
     * https://www.youtube.com/watch?v=onpteKMsE84 (Accessed 3 June 2024)
     * BMo (2022), 'How to Add a TRAIL EFFECT to Anything in Unity', youtube, 2 May
     * https://www.youtube.com/watch?v=nLxvCRPJCKw (Accessed 3 June 2024)
     */
    private IEnumerator SpawnTrail(TrailRenderer tracerTrail, float distance)
    {
        float time = 0f;
        float arrivalTime = distance / tracerSpeed;
        while (time <= arrivalTime) {
            tracerTrail.transform.Translate(Vector3.forward * (tracerSpeed * Time.deltaTime));
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(tracerTrail.gameObject);
    }
}
