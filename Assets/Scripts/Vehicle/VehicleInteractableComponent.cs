using Observer;

namespace Vehicle
{
    public class VehicleInteractableComponent : InteractableComponent
    {
        private SubjectComponent subjectComponent;

        // Start is called before the first frame update
        private void Start()
        {
            subjectComponent = GetComponent<SubjectComponent>();
        }

        public override void Interact()
        {
            // Broadcast events
            subjectComponent.NotifyObservers(new MCEvent(EventType.EnteredVehicle));
            // Because OnTriggerExit() won't trigger this time
            subjectComponent.NotifyObservers(new MCEvent(EventType.InteractionEnded));
        }
    }
}
