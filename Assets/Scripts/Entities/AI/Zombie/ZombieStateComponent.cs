using Entities.Abilities.Health;
using Entities.Abilities.State;
using Entities.AI.Abilities.Perception;
using Entities.AI.Abilities.TargetTracker;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI.Zombie
{
    public class ZombieStateComponent : StateComponent
    {
        [Header("Movement")]
        public float roamSpeed = 2f;
        public float roamPointRadius = 10f;
        public float roamWaitTime = 2f;
        public float chaseSpeed = 4f;

        [Header("Melee Attack")]
        public float attackDamage = 30f;
        public float attackRange = 2f;
        public float attackInterval = 0.5f;

        public NavMeshAgent navMeshAgent;

        public DamageDealerComponent damageDealerComponent;
        public TargetTrackerComponent targetTrackerComponent;
        public ZombieHFSMComponent hfsmComponent;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            damageDealerComponent = GetComponent<DamageDealerComponent>();
            targetTrackerComponent = GetComponent<TargetTrackerComponent>();
            hfsmComponent = GetComponent<ZombieHFSMComponent>();
        }
    }
}
