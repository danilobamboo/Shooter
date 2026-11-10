using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [Header("Percepción")]
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float senseInterval = 0.15f;

    public Transform Player { get; private set; }
    public Health PlayerHealth { get; private set; }

    public bool CanSeePlayer { get; private set; }
    public bool SensedThisFrame { get; private set; }
    public float LastSeenTime { get; private set; }
    public Vector3 LastKnownPlayerPos { get; private set; }

    public Vector3 PlayerTargetPoint => Player.position + Vector3.up * targetHeight;

    private EnemyData data;
    private float nextSenseTime;

    public bool Initialize(EnemyData enemyData)
    {
        data = enemyData;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemySensor: no hay ningún objeto con el tag 'Player' en la escena.", this);
            return false;
        }

        Player = playerObject.transform;
        PlayerHealth = playerObject.GetComponentInParent<Health>();
        if (PlayerHealth == null)
        {
            Debug.LogError("EnemySensor: el jugador necesita un componente Health.", this);
            return false;
        }

        return true;
    }

    public void Tick(bool requireFieldOfView)
    {
        SensedThisFrame = false;
        if (Time.time < nextSenseTime) return;

        nextSenseTime = Time.time + senseInterval;
        SensedThisFrame = true;
        CanSeePlayer = CheckSight(requireFieldOfView);

        if (CanSeePlayer)
        {
            RevealPlayer();
        }
    }

    public void RevealPlayer()
    {
        LastKnownPlayerPos = Player.position;
        LastSeenTime = Time.time;
    }

    private bool CheckSight(bool requireFieldOfView)
    {
        if (PlayerHealth.IsDead) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = PlayerTargetPoint;
        Vector3 toTarget = target - eye;

        if (toTarget.sqrMagnitude > data.sightRange * data.sightRange) return false;

        if (requireFieldOfView)
        {
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            if (Vector3.Angle(transform.forward, flat) > data.fieldOfView * 0.5f) return false;
        }

        return !Physics.Linecast(eye, target, obstacleMask, QueryTriggerInteraction.Ignore);
    }
}