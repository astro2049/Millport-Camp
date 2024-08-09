using Entities.Abilities.Input;
using Entities.Abilities.Observer;
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
            vehicleActionMap = GetComponent<PlayerInput>().actions.FindActionMap("Vehicle");
            vehicleStateComponent = GetComponent<VehicleStateComponent>();
        }

        private void OnEnable()
        {
            vehicleActionMap.Enable();
        }

        private void OnDisable()
        {
            vehicleActionMap.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled) {
                return;
            }

            moveInput = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) {
                return;
            }

            // Broadcast event: Player exits vehicle
            vehicleStateComponent.driver.GetComponent<SubjectComponent>().NotifyObservers(new MCEventWEntity(EventType.ExitedVehicle, gameObject));
            // Clear driver reference
            vehicleStateComponent.driver = null;
        }
    }
}
