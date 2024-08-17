using Entities.Abilities.Input;
using Entities.Abilities.Observer;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Vehicle
{
    public class VehicleInputComponent : InputComponent
    {
        public Vector2 moveInput;

        private VehicleStateComponent vehicleStateComponent;
        private InputActionMap vehicleActionMap;

        private void Awake()
        {
            vehicleStateComponent = GetComponent<VehicleStateComponent>();
            vehicleActionMap = InputManager.instance.gameplayActionMaps.FindActionMap("Vehicle");
        }

        private void OnEnable()
        {
            vehicleActionMap.FindAction("Move").performed += OnMove;
            vehicleActionMap.FindAction("Move").canceled += OnMove;
            vehicleActionMap.FindAction("Interact").performed += OnInteract;
            vehicleActionMap.Enable();
        }

        private void OnDisable()
        {
            vehicleActionMap.FindAction("Move").performed -= OnMove;
            vehicleActionMap.FindAction("Move").canceled += OnMove;
            vehicleActionMap.FindAction("Interact").performed -= OnInteract;
            vehicleActionMap.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // Broadcast event: Player exits vehicle
            vehicleStateComponent.driver.GetComponent<SubjectComponent>().NotifyObservers(new MCEventWEntity(EventType.ExitedVehicle, gameObject));
            // Clear driver reference
            vehicleStateComponent.driver = null;
        }
    }
}
