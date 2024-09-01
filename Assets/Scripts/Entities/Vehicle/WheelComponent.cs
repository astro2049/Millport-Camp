using UnityEngine;

namespace Entities.Vehicle
{
    /*
     * Tire Control, modified from:
     * Unity (2024) Create a car with Wheel colliders.
     * Available at: https://docs.unity3d.com/Manual/WheelColliderTutorial.html (Accessed: 7 June 2024).
     */
    public class WheelComponent : MonoBehaviour
    {
        [HideInInspector] public WheelCollider wheelCollider;
        public bool steerable;
        [SerializeField] private Transform wheelModelTransform;

        private Vector3 position;
        private Quaternion rotation;

        private void Awake()
        {
            wheelCollider = GetComponent<WheelCollider>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Set the wheel's model to same position and rotation as collider
            wheelCollider.GetWorldPose(out position, out rotation);
            wheelModelTransform.position = position;
            wheelModelTransform.rotation = rotation;
        }
    }
}
