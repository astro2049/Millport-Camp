using UnityEngine;

namespace Vehicle
{
    /*
     * Vehicle Control, modified from
     * Unity (2024) Create a car with Wheel colliders
     * https://docs.unity3d.com/Manual/WheelColliderTutorial.html (Accessed: 7 June 2024)
     */
    public class VehicleControlComponent : MonoBehaviour
    {
        public float motorTorque = 2000;
        public float brakeTorque = 2000;
        public float maxSpeed = 20;
        public float steeringRange = 30;
        public float steeringRangeAtMaxSpeed = 10;
        public float centreOfGravityOffset = -1f;

        private TireControlComponent[] tires;
        private Rigidbody rigidBody;

        private VehicleInputComponent vehicleInputComponent;

        // Start is called before the first frame update
        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();

            // Adjust center of mass vertically, to help prevent the car from rolling
            rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

            // Find all child GameObjects that have the TireControl script attached
            tires = GetComponentsInChildren<TireControlComponent>();

            vehicleInputComponent = GetComponent<VehicleInputComponent>();
        }

        // Update is called once per frame
        private void Update()
        {
            float vInput = vehicleInputComponent.moveInput.y;
            float hInput = vehicleInputComponent.moveInput.x;

            // Calculate current speed in relation to the forward direction of the car
            // (this returns a negative number when traveling backwards)
            float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

            // Calculate how close the car is to top speed as a number from zero to one
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

            // Use that to calculate how much torque is available (zero torque at top speed)
            float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
            // …and to calculate how much to steer (the car steers more gently at top speed)
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

            // Check whether the user input is in the same direction as the car's velocity
            bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

            foreach (TireControlComponent tire in tires) {
                // Apply steering to Wheel colliders that have "Steerable" enabled
                if (tire.steerable) {
                    tire.tireCollider.steerAngle = hInput * currentSteerRange;
                }

                if (isAccelerating) {
                    tire.tireCollider.motorTorque = vInput * currentMotorTorque;
                    tire.tireCollider.brakeTorque = 0;
                } else {
                    // If the user is trying to go in the opposite direction
                    // apply brakes to all tires
                    tire.tireCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                    tire.tireCollider.motorTorque = 0;
                }
            }
        }
    }
}
