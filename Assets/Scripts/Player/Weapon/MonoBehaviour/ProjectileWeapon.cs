using UnityEngine;

public class ProjectileWeapon : Weapon
{
    private const float AimDistance = 1000f;

    private ProjectileWeaponData projectileData;
    private Collider[] ownerColliders;

    protected override void Awake()
    {
        base.Awake();
        if (data == null) return;

        projectileData = data as ProjectileWeaponData;
        if (projectileData == null)
        {
            Debug.LogError("ProjectileWeapon: el Data debe ser un ProjectileWeaponData " +
                           "(Create > Weapons > Projectile Weapon Data).", this);
            enabled = false;
            return;
        }

        if (projectileData.projectilePrefab == null)
        {
            Debug.LogError("ProjectileWeapon: el ProjectileWeaponData no tiene un prefab de bala asignado.", this);
            enabled = false;
            return;
        }

        ownerColliders = transform.root.GetComponentsInChildren<Collider>(true);
    }

    protected override void Shoot()
    {
        Vector3 origin = muzzle.position;
        Vector3 direction = GetAimDirection(origin);

        Projectile bullet = Instantiate(projectileData.projectilePrefab, origin, Quaternion.LookRotation(direction));
        bullet.Launch(direction, projectileData, ownerColliders);
    }

    private Vector3 GetAimDirection(Vector3 origin)
    {
        Transform cam = playerCamera.transform;
        Vector3 aimPoint = cam.position + cam.forward * AimDistance;

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, AimDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Vector3 toAim = aimPoint - origin;
        return toAim.sqrMagnitude < 1f ? cam.forward : toAim.normalized;
    }
}