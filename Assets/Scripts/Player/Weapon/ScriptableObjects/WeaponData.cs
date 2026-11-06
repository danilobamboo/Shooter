using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName = "Pistola";

    [Header("Disparo")]
    public float damage = 25f;
    public float range = 100f;
    public float fireDelay = 0.25f;
    public bool automatic = false;

    [Header("Munición")]
    [Min(1)] public int magazineSize = 12;
    [Min(0.1f)] public float reloadTime = 2f;

    [Header("Trazador")]
    public Color tracerColor = Color.yellow;
    public float tracerWidth = 0.02f;
    public float tracerDuration = 0.05f;
}