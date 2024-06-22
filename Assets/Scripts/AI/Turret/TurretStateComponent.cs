using Gun;
using TMPro;
using UnityEngine;

namespace AI.Turret
{
    public class TurretStateComponent : StateComponent
    {
        public float turnSpeed = 360f; // 360 deg/s
        
        public float perceptionRadius = 5f;
        public LayerMask[] perceptionLayers;

        public Transform baseTransform;
        public GunStateComponent gun;

        public TextMeshPro ammoText;

        private void Awake()
        {
            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);
        }
    }
}
