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

    [Header("Trazado")]
    public Color tracerColor = Color.yellow;
    public float tracerWidth = 0.02f;
    public float tracerDuration = 0.05f;
}