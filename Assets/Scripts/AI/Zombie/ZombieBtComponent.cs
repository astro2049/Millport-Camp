using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace AI.Zombie
{
    public class ZombieBtComponent : BtComponent
    {
        /*
     * Pre-stored components
     */
        private ZombieStateComponent zombieStateComponent;
        private NavMeshAgent navMeshAgent;
        private DamageDealerComponent damageDealerComponent;

        // Start is called before the first frame update
        private void Start()
        {
            // Pre-store Components
            navMeshAgent = GetComponent<NavMeshAgent>();
            zombieStateComponent = GetComponent<ZombieStateComponent>();
            damageDealerComponent = GetComponent<DamageDealerComponent>();

            // Initialize and start behavior tree
            InitializeBt();
            bt.Start();
        #if UNITY_EDITOR
            Debugger debugger = (Debugger)gameObject.AddComponent(typeof(Debugger));
            debugger.BehaviorTree = bt;
        #endif
        }

        protected override void InitializeBt()
        {
            bt = new Root(
                new Selector(
                    new Condition(() => !zombieStateComponent.isEngaging, Stops.SELF,
                        new Repeater(
                            new Sequence(
                                // patrol
                                new Action(GeneratePatrolPoint),
                                // TODO: Check 'Has Reached Patrol Point' through service
                                new Action(MoveToPatrolPoint),
                                new Wait(Random.Range(2, 3))
                            )
                        )
                    ),
                    new Condition(() => zombieStateComponent.isEngaging, Stops.SELF,
                        new Sequence(
                            // engage
                            new Action(StartChasing),
                            // TODO: Check 'Can Attack' through service
                            new Action(ChaseHuman),
                            new Action(Attack)
                        )
                    )
                )
            );
        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void GeneratePatrolPoint()
        {
            navMeshAgent.speed = zombieStateComponent.roamSpeed;
            // Generate a new random location within a range
            Vector3 randomLocation = transform.position + Random.insideUnitSphere * zombieStateComponent.randomPatrolRadius;
            NavMesh.SamplePosition(randomLocation, out NavMeshHit hit, zombieStateComponent.randomPatrolRadius, 1);
            navMeshAgent.SetDestination(hit.position);
        }

        private Action.Result MoveToPatrolPoint(bool aborted)
        {
            if (aborted) {
                navMeshAgent.ResetPath();
                return Action.Result.FAILED;
            }
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                return Action.Result.SUCCESS;
            } else {
                return Action.Result.PROGRESS;
            }
        }

        private void StartChasing()
        {
            navMeshAgent.speed = zombieStateComponent.chaseSpeed;
        }

        private Action.Result ChaseHuman(bool aborted)
        {
            if (aborted || !zombieStateComponent.isEngaging) {
                navMeshAgent.ResetPath();
                return Action.Result.FAILED;
            }
            navMeshAgent.SetDestination(zombieStateComponent.humans.Min.transform.position);
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= zombieStateComponent.attackRange) {
                return Action.Result.SUCCESS;
            } else {
                return Action.Result.PROGRESS;
            }
        }

        private void Attack()
        {
            damageDealerComponent.DealDamage(zombieStateComponent.humans.Min, 100f);
            // TODO: Reset combat status, this is hacky
            zombieStateComponent.isEngaging = false;
        }
    }
}
