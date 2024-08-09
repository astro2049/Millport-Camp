using Entities.Abilities.Interactable;
using Entities.Abilities.Observer;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Vehicle
{
    public class VehicleInteractableComponent : InteractableComponent
    {
        private VehicleStateComponent vehicleStateComponent;

        private void Awake()
        {
            vehicleStateComponent = GetComponent<VehicleStateComponent>();
        }

        // Player enters vehicle here
        public override void Interact(GameObject initiator)
        {
            // Set player as driver
            vehicleStateComponent.driver = initiator;
            // Broadcast events
            initiator.GetComponent<SubjectComponent>().NotifyObservers(new MCEventWEntity(EventType.EnteredVehicle, gameObject));
            // Because OnTriggerExit() won't trigger this time
            initiator.GetComponent<SubjectComponent>().NotifyObservers(new MCEvent(EventType.InteractionEnded));
        }
    }
}
