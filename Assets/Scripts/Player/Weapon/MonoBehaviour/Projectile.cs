using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    private float damage;
    private float gravityScale;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, ProjectileWeaponData data, Collider[] ownerColliders)
    {
        damage = data.damage;
        gravityScale = data.gravityScale;

        rb.isKinematic = false;
        rb.useGravity = false;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        IgnoreOwner(ownerColliders);

        rb.AddForce(direction * data.projectileSpeed, ForceMode.VelocityChange);

        Destroy(gameObject, data.range / Mathf.Max(data.projectileSpeed, 1f));
    }

    private void FixedUpdate()
    {
        if (gravityScale > 0f)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        collision.collider.GetComponentInParent<IDamageable>()?.TakeDamage(damage);

        Destroy(gameObject);
    }

    private void IgnoreOwner(Collider[] ownerColliders)
    {
        if (ownerColliders == null) return;

        Collider[] myColliders = GetComponentsInChildren<Collider>();

        foreach (Collider mine in myColliders)
        {
            foreach (Collider other in ownerColliders)
            {
                if (other != null) Physics.IgnoreCollision(mine, other);
            }
        }
    }
}