namespace Vehicle
{
    public class VehicleInteractableComponent : InteractableComponent
    {
        public override void Interact()
        {
            GetComponent<VehicleStateComponent>().gameManager.EnterVehicle(gameObject);

            // Because OnTriggerExit() won't trigger this time
            GetComponent<VehicleStateComponent>().gameManager.CloseInteraction();
        }
    }
}
