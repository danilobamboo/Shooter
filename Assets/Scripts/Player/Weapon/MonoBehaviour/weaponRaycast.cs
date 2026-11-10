using System;
using UnityEngine;

public class HitscanWeapon : MonoBehaviour
{
    [Header("Datos del arma")]
    [SerializeField] private WeaponData data;

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer tracer;

    [Header("Física")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Controles")]
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    public event Action<int, int> OnAmmoChanged;

    public int CurrentAmmo { get; private set; }
    public int MagazineSize => data != null ? data.magazineSize : 0;
    public bool IsReloading { get; private set; }

    public float ReloadProgress
    {
        get
        {
            if (!IsReloading) return 0f;
            float remaining = reloadEndTime - Time.time;
            return Mathf.Clamp01(1f - remaining / data.reloadTime);
        }
    }

    private float nextFireTime;
    private float reloadEndTime;
    private float tracerHideTime;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError("HitscanWeapon: falta asignar un WeaponData.", this);
            enabled = false;
            return;
        }

        if (playerCamera == null) playerCamera = Camera.main;
        if (muzzle == null) muzzle = transform;

        if (tracer == null) tracer = CreateTracer();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.enabled = false;

        CurrentAmmo = data.magazineSize;
    }

    private void OnEnable()
    {
        if (data != null && CurrentAmmo <= 0 && !IsReloading)
        {
            StartReload();
        }
    }

    private void OnDisable()
    {

        IsReloading = false;

        if (tracer != null) tracer.enabled = false;
    }

    private void Update()
    {
        if (IsReloading)
        {
            if (Time.time >= reloadEndTime) FinishReload();
        }
        else if (Input.GetKeyDown(reloadKey) && CurrentAmmo < data.magazineSize)
        {
            StartReload();
        }
        else
        {
            bool wantsToFire = data.automatic ? Input.GetButton("Fire1")
                                              : Input.GetButtonDown("Fire1");

            if (wantsToFire && Time.time >= nextFireTime)
            {
                Fire();
            }
        }

        if (tracer.enabled && Time.time >= tracerHideTime)
        {
            tracer.enabled = false;
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + data.fireDelay;

        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo, data.magazineSize);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 endPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, data.range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

        }
        else
        {
            endPoint = ray.origin + ray.direction * data.range;
        }

        ShowTracer(muzzle.position, endPoint);

        if (CurrentAmmo <= 0)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        IsReloading = true;
        reloadEndTime = Time.time + data.reloadTime;
    }

    private void FinishReload()
    {

        IsReloading = false;
        CurrentAmmo = data.magazineSize;
        OnAmmoChanged?.Invoke(CurrentAmmo, data.magazineSize);
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