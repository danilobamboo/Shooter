using UnityEngine;

public class HitscanWeapon : Weapon
{
    [SerializeField] private LineRenderer tracer;

    private float tracerHideTime;

    protected override void Awake()
    {
        base.Awake();
        if (data == null) return;

        if (tracer == null) tracer = CreateTracer();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.enabled = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (tracer != null) tracer.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (tracer.enabled && Time.time >= tracerHideTime)
        {
            tracer.enabled = false;
        }
    }

    protected override void Shoot()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 endPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, data.range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(data.damage);
        }
        else
        {
            endPoint = ray.origin + ray.direction * data.range;
        }

        ShowTracer(muzzle.position, endPoint);
    }

    private void ShowTracer(Vector3 start, Vector3 end)
    {
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        tracer.enabled = true;
        tracerHideTime = Time.time + data.tracerDuration;
    }

    private LineRenderer CreateTracer()
    {
        GameObject go = new GameObject("Tracer");
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = data.tracerWidth;
        lr.endWidth = data.tracerWidth;
        lr.startColor = data.tracerColor;
        lr.endColor = data.tracerColor;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }
}