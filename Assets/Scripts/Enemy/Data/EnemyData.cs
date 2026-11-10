using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    public string enemyName = "Enemigo";
    [Min(1f)] public float maxHealth = 100f;

    [Header("Movimiento")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;

    [Header("Patrulla")]
    public float waitAtPoint = 1.5f;

    [Header("Detección")]
    public float sightRange = 18f;
    [Range(10f, 360f)] public float fieldOfView = 110f;
    public float loseSightTime = 4f;

    [Header("Combate")]
    public WeaponData weapon;
    public float combatRange = 12f;
    [Range(0f, 15f)] public float aimSpreadAngle = 4f;
    public float reactionTime = 0.5f;

    [Header("Esquive lateral (strafe)")]
    public float strafeSpeed = 3f;
    public float strafeDistance = 3f;
    public Vector2 strafeInterval = new Vector2(0.8f, 1.6f);
}