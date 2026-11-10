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

    [Header("Ataque (cuerpo a cuerpo)")]
    public float attackRange = 1.8f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;
    public float attackWindup = 0.4f;
}