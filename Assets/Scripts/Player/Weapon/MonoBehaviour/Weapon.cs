using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Datos del arma")]
    [SerializeField] protected WeaponData data;

    [Header("Referencias")]
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected Transform muzzle;

    [Header("Física")]
    [SerializeField] protected LayerMask hitMask = ~0;

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

    protected abstract void Shoot();

    protected virtual void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"{GetType().Name}: falta asignar un WeaponData.", this);
            enabled = false;
            return;
        }

        if (playerCamera == null) playerCamera = Camera.main;
        if (muzzle == null) muzzle = transform;

        CurrentAmmo = data.magazineSize;
    }

    protected virtual void OnEnable()
    {
        if (data != null && CurrentAmmo <= 0 && !IsReloading)
        {
            StartReload();
        }
    }

    protected virtual void OnDisable()
    {
        IsReloading = false;
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0f) return;

        if (IsReloading)
        {
            if (Time.time >= reloadEndTime) FinishReload();
            return;
        }

        if (Input.GetKeyDown(reloadKey) && CurrentAmmo < data.magazineSize)
        {
            StartReload();
            return;
        }

        bool wantsToFire = data.automatic ? Input.GetButton("Fire1")
                                          : Input.GetButtonDown("Fire1");

        if (wantsToFire && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    private void Fire()
    {
        nextFireTime = Time.time + data.fireDelay;

        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo, data.magazineSize);

        Shoot();

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
}