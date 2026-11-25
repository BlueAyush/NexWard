
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;

    private bool hasHit;
    private bool isPiercing;

    private HashSet<IDamageable> hitTargets =
    new HashSet<IDamageable>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    public void Launch(Vector2 direction, float damageAmount)
    {
        Launch(direction, damageAmount, false);
    }

    public void Launch(
        Vector2 direction,
        float damageAmount,
        bool piercing)
    {
        damage = damageAmount;
        isPiercing = piercing;

        Launch(direction);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target =
            other.GetComponentInParent<IDamageable>();

        if (target == null || !target.IsAlive)
            return;

        if (isPiercing)
        {
            if (!hitTargets.Add(target))
                return;

            target.TakeDamage(damage);
        }
        else
        {
            if (hasHit)
                return;

            hasHit = true;

            GetComponent<Collider2D>().enabled = false;

            target.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

}
