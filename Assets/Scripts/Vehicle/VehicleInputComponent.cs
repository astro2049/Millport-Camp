using Observer;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Observer.EventType;

namespace Vehicle
{
    public class VehicleInputComponent : MonoBehaviour
    {
        public InputActionAsset inputActionAsset;
        public Vector2 moveInput;

        private InputActionMap vehicleActionMap;
        private SubjectComponent subjectComponent;

        private void Awake()
        {
            vehicleActionMap = inputActionAsset.FindActionMap("Vehicle");

            // Bind input functions
            // Move
            vehicleActionMap.FindAction("Move").performed += OnMove;
            vehicleActionMap.FindAction("Move").canceled += OnMove;
            // Interact
            vehicleActionMap.FindAction("Interact").performed += OnInteract;
        }

        private void OnEnable()
        {
            vehicleActionMap.Enable();
        }

        private void OnDisable()
        {
            vehicleActionMap.Disable();
        }

        // Start is called before the first frame update
        private void Start()
        {
            subjectComponent = GetComponent<SubjectComponent>();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // Broadcast event
            subjectComponent.NotifyObservers(EventType.ExitedVehicle);
        }
    }
}
