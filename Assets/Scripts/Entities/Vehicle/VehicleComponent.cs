using System;
using TMPro;
using UnityEngine;

namespace Entities.Vehicle
{
    /*
     * Vehicle Control, modified from:
     * Unity (2024) Create a car with Wheel colliders.
     * Available at: https://docs.unity3d.com/Manual/WheelColliderTutorial.html (Accessed: 7 June 2024).
     */
    public class VehicleComponent : MonoBehaviour
    {
        [SerializeField] private float motorTorque = 2000;
        [SerializeField] private float cruisingBrakeTorque = 2000;
        [SerializeField] private float brakeTorque = 5000;
        // max speed: forward and backward
        [SerializeField] private float maxSpeed = 30;
        [SerializeField] private float steeringRange = 45;
        [SerializeField] private float steeringRangeAtMaxSpeed = 35;
        [SerializeField] private float centreOfGravityOffset = -1f;

        private Rigidbody rigidBody;
        private VehicleInputComponent vehicleInputComponent;
        private WheelComponent[] wheels;

        [SerializeField] private TextMeshPro powerModeText;

        private void Awake()
        {
            // Get components
            rigidBody = GetComponent<Rigidbody>();
            vehicleInputComponent = GetComponent<VehicleInputComponent>();
            wheels = GetComponentsInChildren<WheelComponent>();

            // Adjust center of mass vertically, to help prevent the car from rolling
            rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        }

        // Update is called once per frame
        private void Update()
        {
            float forwardInput = vehicleInputComponent.moveInput.y;
            float turnInput = vehicleInputComponent.moveInput.x;

            // Calculate current forward speed
            float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
            // How fast is the vehicle comparing to top speed? Range: [0, 1]
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

            // Calculate steer angle. The car steers more gently at top speed.
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

            // Calculate motor torque and brake torque
            float wheelMotorTorque, wheelBrakeTorque;
            if (forwardInput == 0) {
                // Power Mode 0 - Cruising
                wheelMotorTorque = 0;
                wheelBrakeTorque = cruisingBrakeTorque;
                powerModeText.text = "Cruising";
            } else if (Math.Abs(Mathf.Sign(forwardInput) - Mathf.Sign(forwardSpeed)) < 1e-4) {
                // Power Mode 1 - Want to accelerate (either backward or forward)
                // How much torque is available? 0 torque @ top speed
                float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
                wheelMotorTorque = forwardInput * currentMotorTorque;
                wheelBrakeTorque = 0;
                powerModeText.text = "Accelerating";
            } else {
                // Power Mode 2 - Want to brake
                wheelMotorTorque = 0;
                wheelBrakeTorque = brakeTorque;
                powerModeText.text = "Braking";
            }

            // Apply torques to wheels
            foreach (WheelComponent wheel in wheels) {
                // Apply steering angle
                if (wheel.steerable) {
                    wheel.wheelCollider.steerAngle = turnInput * currentSteerRange;
                }
                // Apply motor and brake torques
                wheel.wheelCollider.motorTorque = wheelMotorTorque;
                wheel.wheelCollider.brakeTorque = wheelBrakeTorque;
            }
        }
    }
}
