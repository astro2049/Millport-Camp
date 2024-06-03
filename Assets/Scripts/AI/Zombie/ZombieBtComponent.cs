using NPBehave;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ZombieBtComponent : BtComponent
{
    public float roamSpeed = 2f; // 2m/s
    public float chaseSpeed = 4f; // 4m/s
    public float randomPatrolRadius = 10f; // 10m
    public float attackRange = 2f; // 2m

    /*
     * Pre-stored components
     */
    private NavMeshAgent navMeshAgent;
    private ZombieStateComponent zombieStateComponent;
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
        navMeshAgent.speed = roamSpeed;
        // Generate a new random location within a range
        Vector3 randomLocation = transform.position + Random.insideUnitSphere * randomPatrolRadius;
        NavMesh.SamplePosition(randomLocation, out NavMeshHit hit, randomPatrolRadius, 1);
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
        navMeshAgent.speed = chaseSpeed;
    }

    private Action.Result ChaseHuman(bool aborted)
    {
        if (aborted || !zombieStateComponent.isEngaging) {
            navMeshAgent.ResetPath();
            return Action.Result.FAILED;
        }
        navMeshAgent.SetDestination(zombieStateComponent.humans.Min.transform.position);
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= attackRange) {
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
