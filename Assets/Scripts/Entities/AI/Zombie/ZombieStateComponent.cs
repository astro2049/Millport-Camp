using System.Collections.Generic;
using Entities.Abilities.State;
using Entities.AI.Abilities.Perception;
using UnityEngine;

namespace Entities.AI.Zombie
{
    public class ZombieStateComponent : StateComponent
    {
        public float roamSpeed = 2f;
        public float chaseSpeed = 4f;
        public float randomPatrolRadius = 10f;
        public float attackRange = 2f;
        public float perceptionRadius = 7.5f;
        public LayerMask perceptionLayers;

        public HashSet<GameObject> humans = new HashSet<GameObject>();
        public bool isEngaging;

        private void Awake()
        {
            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);
        }
    }
}
