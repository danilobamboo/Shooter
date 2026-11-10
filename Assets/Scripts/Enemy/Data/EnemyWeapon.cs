using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fallbackMuzzleHeight = 1.4f;

    [Header("Física")]
    [SerializeField] private LayerMask shotMask = ~0;

    private WeaponData weapon;
    private float spreadAngle;
    private LineRenderer tracer;
    private float nextShotTime;
    private float tracerHideTime;

    private Vector3 MuzzlePosition => muzzle != null
        ? muzzle.position
        : transform.position + Vector3.up * fallbackMuzzleHeight + transform.forward * 0.5f;

    public void Initialize(EnemyData data)
    {
        weapon = data.weapon;
        spreadAngle = data.aimSpreadAngle;

        tracer = CreateTracer();
        tracer.enabled = false;
    }

    public void DelayNextShot(float seconds)
    {
        nextShotTime = Mathf.Max(nextShotTime, Time.time + seconds);
    }

    public void TryShoot(Vector3 targetPoint)
    {
        if (weapon == null || Time.time < nextShotTime) return;
        nextShotTime = Time.time + weapon.fireDelay;

        Vector3 origin = MuzzlePosition;
        Vector3 direction = ApplySpread((targetPoint - origin).normalized, spreadAngle);
        Vector3 endPoint = origin + direction * weapon.range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, weapon.range, shotMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(weapon.damage);
        }

        ShowTracer(origin, endPoint);
    }

    private void Update()
    {
        if (tracer != null && tracer.enabled && Time.time >= tracerHideTime)
        {
            tracer.enabled = false;
        }
    }

    private static Vector3 ApplySpread(Vector3 direction, float angle)
    {
        if (angle <= 0f) return direction;

        Vector2 deviation = Random.insideUnitCircle * angle;
        Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(deviation.y, deviation.x, 0f);
        return rotation * Vector3.forward;
    }

    private void ShowTracer(Vector3 start, Vector3 end)
    {
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        tracer.enabled = true;
        tracerHideTime = Time.time + weapon.tracerDuration;
    }

    private LineRenderer CreateTracer()
    {
        GameObject go = new GameObject("EnemyTracer");
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = weapon.tracerWidth;
        lr.endWidth = weapon.tracerWidth;
        lr.startColor = weapon.tracerColor;
        lr.endColor = weapon.tracerColor;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }
}