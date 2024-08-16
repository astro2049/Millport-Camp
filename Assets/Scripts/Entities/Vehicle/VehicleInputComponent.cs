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

        public void OnMove(InputValue inputValue)
        {
            moveInput = inputValue.Get<Vector2>();
        }

        public void OnInteract()
        {
            // Broadcast event: Player exits vehicle
            vehicleStateComponent.driver.GetComponent<SubjectComponent>().NotifyObservers(new MCEventWEntity(EventType.ExitedVehicle, gameObject));
            // Clear driver reference
            vehicleStateComponent.driver = null;
        }
    }
}
