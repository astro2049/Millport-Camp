using System.Collections.Generic;
using UnityEngine;

namespace AI.Zombie
{
    public class ZombieStateComponent : AIStateComponent
    {
        public float roamSpeed = 2f;
        public float chaseSpeed = 4f;
        public float randomPatrolRadius = 10f;
        public float attackRange = 2f;
        public float perceptionRadius = 7.5f;
    
        public SortedSet<GameObject> humans = new SortedSet<GameObject>();
        public bool isEngaging;

        // Start is called before the first frame update
        private void Start()
        {
            transform.Find("Perception Collider").gameObject.GetComponent<SphereCollider>().radius = perceptionRadius;
            GetComponent<ZombieBtComponent>().enabled = true;
        }

        // Update is called once per frame
        private void Update()
        {

        }

        public override void OnPerceptionTriggerEnter(Collider other)
        {
            // Debug.Log(other.gameObject.name + " Entered");
            humans.Add(other.gameObject);
            isEngaging = true;
        }

        public override void OnPerceptionTriggerExit(Collider other)
        {
            // Debug.Log(other.gameObject.name + " Exited");
            humans.Remove(other.gameObject);
            if (humans.Count == 0) {
                isEngaging = false;
            }
        }
    }
}
