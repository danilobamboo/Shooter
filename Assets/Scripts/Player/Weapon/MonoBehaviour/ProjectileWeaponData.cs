using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileWeapon", menuName = "Weapons/Projectile Weapon Data")]
public class ProjectileWeaponData : WeaponData
{
    [Header("Proyectil")]
    public Projectile projectilePrefab;
    public float projectileSpeed = 150f;
    [Range(0f, 1f)] public float gravityScale = 0.15f;
}