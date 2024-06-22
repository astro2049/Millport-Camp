using System.Collections.Generic;
using UnityEngine;

namespace AI.NPC.Zombie
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

        // Start is called before the first frame update
        private void Start()
        {
            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);

            // Enable behavior tree
            GetComponent<ZombieBtComponent>().enabled = true;
        }

        // Update is called once per frame
        private void Update()
        {

        }
    }
}
