using System.Collections.Generic;
using Entities.Abilities.AI.Perception;
using Entities.Abilities.State;
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
        public LayerMask[] perceptionLayers;

        public SortedSet<GameObject> humans = new SortedSet<GameObject>();
        public bool isEngaging;

        private void Awake()
        {
            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Enable behavior tree
            GetComponent<ZombieBtComponent>().enabled = true;
        }

        // Update is called once per frame
        private void Update()
        {

        }
    }
}
