using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health), typeof(EnemySensor))]
[RequireComponent(typeof(EnemyWeapon))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Combat, Dead }

    [Header("Datos")]
    [SerializeField] private EnemyData data;

    [Header("Zona de patrulla")]
    [SerializeField] private Transform patrolCenter;
    [SerializeField] private float patrolRadius = 8f;

    [Header("Otros")]
    [SerializeField] private float attackTurnSpeed = 540f;
    [SerializeField] private float destroyDelay = 2f;

    private NavMeshAgent agent;
    private Health health;
    private EnemySensor sensor;
    private EnemyWeapon gun;

    private State state;
    private Vector3 homePosition;

    private bool waiting;
    private float waitUntil;

    private float nextStrafeTime;
    private float strafeSide = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        sensor = GetComponent<EnemySensor>();
        gun = GetComponent<EnemyWeapon>();

        if (sensor == null || gun == null)
        {
            Debug.LogError("EnemyAI: faltan los componentes EnemySensor y/o EnemyWeapon en este objeto.", this);
            enabled = false;
            return;
        }

        if (data == null)
        {
            Debug.LogError("EnemyAI: falta asignar un EnemyData.", this);
            enabled = false;
            return;
        }

        if (data.weapon == null)
        {
            Debug.LogError("EnemyAI: el EnemyData no tiene un arma (WeaponData) asignada.", this);
            enabled = false;
            return;
        }

        if (!sensor.Initialize(data))
        {
            enabled = false;
            return;
        }

        gun.Initialize(data);

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

        sensor.Tick(state == State.Patrol);

        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Combat: UpdateCombat(); break;
        }
    }


    private void EnterPatrol()
    {
        state = State.Patrol;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.speed = data.patrolSpeed;
        agent.stoppingDistance = 0.2f;
        waiting = false;
        GoToRandomPatrolPoint();
    }

    private void UpdatePatrol()
    {
        if (sensor.CanSeePlayer)
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
        agent.updateRotation = true;
        agent.speed = data.chaseSpeed;
        agent.stoppingDistance = 0.5f;
        agent.SetDestination(sensor.LastKnownPlayerPos);
    }

    private void UpdateChase()
    {
        if (sensor.SensedThisFrame)
        {
            agent.SetDestination(sensor.LastKnownPlayerPos);
        }

        if (sensor.CanSeePlayer && FlatDistanceToPlayer() <= data.combatRange)
        {
            EnterCombat();
            return;
        }

        if (!sensor.CanSeePlayer && Time.time - sensor.LastSeenTime > data.loseSightTime)
        {
            EnterPatrol();
        }
    }


    private void EnterCombat()
    {
        state = State.Combat;
        agent.isStopped = false;
        agent.ResetPath();                
        agent.updateRotation = false;     
        agent.speed = data.strafeSpeed;
        agent.stoppingDistance = 0.1f;

        gun.DelayNextShot(data.reactionTime);
        nextStrafeTime = Time.time;       
    }

    private void UpdateCombat()
    {
        FaceTarget();

        if (!sensor.CanSeePlayer || FlatDistanceToPlayer() > data.combatRange * 1.2f)
        {
            EnterChase();
            return;
        }

        gun.TryShoot(sensor.PlayerTargetPoint);

        if (data.strafeSpeed > 0f && Time.time >= nextStrafeTime)
        {
            StartNewStrafe();
        }
    }

    private void StartNewStrafe()
    {
        nextStrafeTime = Time.time + Random.Range(data.strafeInterval.x, data.strafeInterval.y);

        Vector3 toPlayer = sensor.Player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f) return;

        Vector3 right = Vector3.Cross(Vector3.up, toPlayer.normalized);

        float side = Random.value < 0.7f ? -strafeSide : strafeSide;

        Vector3 point;
        if (TryGetStrafePoint(right * side, out point))
        {
            strafeSide = side;
            agent.SetDestination(point);
        }
        else if (TryGetStrafePoint(right * -side, out point))
        {
            strafeSide = -side;
            agent.SetDestination(point);
        }
    }

    private bool TryGetStrafePoint(Vector3 direction, out Vector3 point)
    {
        point = transform.position;
        Vector3 candidate = transform.position + direction * data.strafeDistance;

        if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)) return false;

        if (NavMesh.Raycast(transform.position, hit.position, out NavMeshHit edge, NavMesh.AllAreas)) return false;

        point = hit.position;
        return true;
    }


    private void HandleDamaged(float amount)
    {
        if (state == State.Dead || health.IsDead) return;

        sensor.RevealPlayer();

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
        Vector3 delta = sensor.Player.position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private void FaceTarget()
    {
        Vector3 direction = sensor.Player.position - transform.position;
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
        DrawCircle(transform.position, data.combatRange);
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