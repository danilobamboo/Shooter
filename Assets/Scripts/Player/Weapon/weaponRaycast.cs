using UnityEngine;

public class HitscanWeapon : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private WeaponData data;

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer tracer;

    [Header("Física")]
    [SerializeField] private LayerMask hitMask = ~0;

    private float nextFireTime;
    private float tracerHideTime;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError("HitscanWeapon: macana.", this);
            enabled = false;
            return;
        }

        if (playerCamera == null) playerCamera = Camera.main;
        if (muzzle == null) muzzle = transform;

        if (tracer == null) tracer = CreateTracer();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.enabled = false;
    }

    private void Update()
    {
        bool wantsToFire = data.automatic ? Input.GetButton("Fire1")
                                          : Input.GetButtonDown("Fire1");

        if (wantsToFire && Time.time >= nextFireTime)
        {
            Fire();
        }

        if (tracer.enabled && Time.time >= tracerHideTime)
        {
            tracer.enabled = false;
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + data.fireDelay;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 endPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, data.range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            Debug.Log($"{data.weaponName} impactó en {hit.collider.name} ({data.damage} de daño)");
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