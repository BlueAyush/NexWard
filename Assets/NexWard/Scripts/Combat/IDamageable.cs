
using UnityEngine;

public interface IDamageable
{
    Transform DamageableTransform { get; }

    bool IsAlive { get; }

    void TakeDamage(float amount);
}
