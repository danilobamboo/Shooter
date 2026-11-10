using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Dead }

    [Header("Datos")]
    [SerializeField] private EnemyData data;

    [Header("Zona de patrulla")]
    [SerializeField] private Transform patrolCenter;
    [SerializeField] private float patrolRadius = 8f;

    [Header("Percepción")]
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float senseInterval = 0.15f;

    [Header("Otros")]
    [SerializeField] private float attackTurnSpeed = 540f;
    [SerializeField] private float destroyDelay = 2f;

    private NavMeshAgent agent;
    private Health health;
    private Transform player;
    private Health playerHealth;

    private State state;
    private Vector3 homePosition;

    private float nextSenseTime;
    private bool sensedThisFrame;
    private bool canSeePlayer;
    private float lastSeenTime;
    private Vector3 lastKnownPlayerPos;

    private bool waiting;
    private float waitUntil;
    private float nextAttackTime;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        if (data == null)
        {
            Debug.LogError("EnemyAI: falta asignar un EnemyData.", this);
            enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemyAI: no hay ningún objeto con el tag 'Player' en la escena.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;
        playerHealth = playerObject.GetComponentInParent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError("EnemyAI: el jugador necesita un componente Health.", this);
            enabled = false;
            return;
        }

        health.Initialize(data.maxHealth);
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("EnemyAI: el enemigo no está sobre un NavMesh. ¿Horneaste el NavMesh?", this);
            enabled = false;
            return;
        }

        homePosition = patrolCenter != null ? patrolCenter.position : transform.position;
        EnterPatrol();
    }


    private void Update()
    {
        if (state == State.Dead) return;

        sensedThisFrame = false;
        if (Time.time >= nextSenseTime)
        {
            nextSenseTime = Time.time + senseInterval;
            sensedThisFrame = true;
            canSeePlayer = CanSeePlayer();

            if (canSeePlayer)
            {
                lastSeenTime = Time.time;
                lastKnownPlayerPos = player.position;
            }
        }

        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private bool CanSeePlayer()
    {
        if (playerHealth.IsDead) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 toTarget = target - eye;

        if (toTarget.sqrMagnitude > data.sightRange * data.sightRange) return false;

        if (state == State.Patrol)
        {
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            if (Vector3.Angle(transform.forward, flat) > data.fieldOfView * 0.5f) return false;
        }

        return !Physics.Linecast(eye, target, obstacleMask, QueryTriggerInteraction.Ignore);
    }


    private void EnterPatrol()
    {
        state = State.Patrol;
        agent.isStopped = false;
        agent.speed = data.patrolSpeed;
        agent.stoppingDistance = 0.2f;
        waiting = false;
        GoToRandomPatrolPoint();
    }

    private void UpdatePatrol()
    {
        if (canSeePlayer)
        {
            EnterChase();
            return;
        }

        if (waiting)
        {
            if (Time.time >= waitUntil)
            {
                waiting = false;
                GoToRandomPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waiting = true;
            waitUntil = Time.time + data.waitAtPoint;
        }
    }

    private void GoToRandomPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = homePosition + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
        waiting = true;
        waitUntil = Time.time + 1f;
    }

    private void EnterChase()
    {
        state = State.Chase;
        waiting = false;
        agent.isStopped = false;
        agent.speed = data.chaseSpeed;
        agent.stoppingDistance = data.attackRange * 0.7f;
        agent.SetDestination(lastKnownPlayerPos);
    }

    private void UpdateChase()
    {
        if (sensedThisFrame)
        {
            agent.SetDestination(lastKnownPlayerPos);
        }

        if (canSeePlayer && FlatDistanceToPlayer() <= data.attackRange)
        {
            EnterAttack();
            return;
        }

        if (!canSeePlayer && Time.time - lastSeenTime > data.loseSightTime)
        {
            EnterPatrol();
        }
    }


    private void EnterAttack()
    {
        state = State.Attack;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        nextAttackTime = Mathf.Max(nextAttackTime, Time.time + data.attackWindup);
    }

    private void UpdateAttack()
    {
        FaceTarget();
        if (!canSeePlayer || FlatDistanceToPlayer() > data.attackRange * 1.25f)
        {
            EnterChase();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + data.attackCooldown;
            playerHealth.TakeDamage(data.attackDamage);
        }
    }

    private void HandleDamaged(float amount)
    {
        if (state == State.Dead || health.IsDead) return;

        lastKnownPlayerPos = player.position;
        lastSeenTime = Time.time;

        if (state == State.Patrol)
        {
            EnterChase();
        }
    }

    private void HandleDied()
    {
        state = State.Dead;
        agent.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        Destroy(gameObject, destroyDelay);
    }


    private float FlatDistanceToPlayer()
    {
        Vector3 delta = player.position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private void FaceTarget()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                                                      attackTurnSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying
            ? homePosition
            : (patrolCenter != null ? patrolCenter.position : transform.position);

        Gizmos.color = Color.cyan;
        DrawCircle(center, patrolRadius);

        if (data == null) return;

        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, data.sightRange);
        Vector3 left = Quaternion.Euler(0f, -data.fieldOfView * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, data.fieldOfView * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(transform.position, left * data.sightRange);
        Gizmos.DrawRay(transform.position, right * data.sightRange);

        Gizmos.color = Color.red;
        DrawCircle(transform.position, data.attackRange);
    }

    private static void DrawCircle(Vector3 center, float radius)
    {
        const int segments = 32;
        Vector3 previous = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}