using Entities.Abilities.Health;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Vehicle
{
    // Works with NavMesh Obstacle
    public class VehicleDamageColliderComponent : MonoBehaviour
    {
        [SerializeField] private NavMeshObstacle carNavMeshObstacle;
        private BoxCollider damageCollider;

        [SerializeField] private Rigidbody carRigidBody;
        [SerializeField] private DamageDealerComponent damageDealerComponent;

        [SerializeField] private float minSpeedToDealDamage = 5f;

        private void Awake()
        {
            damageCollider = GetComponent<BoxCollider>();

            damageCollider.center = carNavMeshObstacle.center;
            damageCollider.size = carNavMeshObstacle.size * 1.05f;
        }

        private void OnTriggerEnter(Collider other)
        {
            Vector3 contactVelocityDirection = Vector3.Normalize(other.transform.position - carRigidBody.transform.position);
            float speed = Vector3.Dot(carRigidBody.velocity, contactVelocityDirection);
            if (speed >= minSpeedToDealDamage) {
                damageDealerComponent.DealDamage(other.gameObject, 100f);
            }
        }
    }
}
