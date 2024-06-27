using UnityEngine;

namespace Entities.Vehicle
{
    /*
     * Tire Control, modified from:
     * Unity (2024) Create a car with Wheel colliders.
     * Available at: https://docs.unity3d.com/Manual/WheelColliderTutorial.html (Accessed: 7 June 2024).
     */
    public class TireControlComponent : MonoBehaviour
    {
        public Transform tireModelTransform;
        [HideInInspector] public WheelCollider tireCollider;
        public bool steerable;

        private Vector3 position;
        private Quaternion rotation;

        // Start is called before the first frame update
        private void Start()
        {
            tireCollider = GetComponent<WheelCollider>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Get the Wheel Collider's world pose values and
            // use them to set the tire model's position and rotation
            tireCollider.GetWorldPose(out position, out rotation);
            tireModelTransform.position = position;
            tireModelTransform.rotation = rotation;
        }
    }
}
