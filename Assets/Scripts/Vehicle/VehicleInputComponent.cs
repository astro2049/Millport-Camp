using UnityEngine;
using UnityEngine.InputSystem;

namespace Vehicle
{
    public class VehicleInputComponent : MonoBehaviour
    {
        public Vector2 moveInput;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void OnInteract()
        {
            GetComponent<VehicleStateComponent>().gameManager.ExitVehicle(gameObject);
        }
    }
}
