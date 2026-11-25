
using UnityEngine;

public class CoreDome : MonoBehaviour
{
    [Header("Dome Settings")]
    [SerializeField] private float radius = 3f;

    [Range(0f, 1f)]
    [SerializeField] private float towerDamageReduction = 0.25f;

    [Header("Visual")]
    [SerializeField] private Transform domeVisual;

    public float Radius => radius;

    public float TowerDamageReduction => towerDamageReduction;

    public bool IsPositionProtected(Vector2 position)
    {
        float distanceSquared =
            ((Vector2)transform.position - position).sqrMagnitude;

        return distanceSquared <= radius * radius;
    }

    public float GetIncomingDamage(float baseDamage, Vector2 towerPosition)
    {
        if (IsPositionProtected(towerPosition))
        {
            return baseDamage * (1f - towerDamageReduction);
        }

        return baseDamage;
    }

    public void IncreaseRadius(float amount)
    {
        radius += amount;
        RefreshVisual();

        Debug.Log($"Dome Radius: {radius}");
    }

    public void IncreaseDamageReduction(float amount)
    {
        towerDamageReduction = Mathf.Clamp01(
            towerDamageReduction + amount
        );

        Debug.Log(
            $"Dome Damage Reduction: {towerDamageReduction * 100}%"
        );
    }

    private void RefreshVisual()
    {
        if (domeVisual == null)
            return;

        domeVisual.position = transform.position;

        float diameter = radius * 2f;
        domeVisual.localScale = new Vector3(diameter, diameter, 1f);
    }

    private void Awake()
    {
        RefreshVisual();
    }

    private void OnValidate()
    {
        RefreshVisual();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
