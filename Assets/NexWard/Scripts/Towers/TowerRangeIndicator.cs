
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerRangeIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerAttack tower;

    [Header("Visual Settings")]
    [SerializeField] private int segments = 64;
    [SerializeField] private float lineWidth = 0.035f;

    [Header("Colors")]
    [SerializeField]
    private Color normalColor =
        new Color(0.2f, 0.8f, 1f, 0.75f);

    private LineRenderer lineRenderer;

    private static Material sharedMaterial;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (tower == null)
            tower = GetComponent<TowerAttack>();

        segments = Mathf.Max(24, segments);

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.sortingOrder = 2;

        // Use a 2D-compatible material.
        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
                sharedMaterial = new Material(shader);
        }

        if (sharedMaterial != null)
            lineRenderer.sharedMaterial = sharedMaterial;

        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;

        lineRenderer.enabled = false;

        Refresh();
    }

    public void Show(bool visible)
    {
        if (tower == null || lineRenderer == null)
            return;

        Refresh();

        lineRenderer.enabled = visible;
    }

    public void SetColor(Color color)
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void Refresh()
    {
        if (tower == null || lineRenderer == null)
            return;

        float radius = tower.AttackRange;

        float scaleX = Mathf.Max(
            Mathf.Abs(transform.lossyScale.x), 0.0001f);

        float scaleY = Mathf.Max(
            Mathf.Abs(transform.lossyScale.y), 0.0001f);

        float radiusX = radius / scaleX;
        float radiusY = radius / scaleY;

        for (int i = 0; i < segments; i++)
        {
            float angle =
                (i / (float)segments) * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * radiusX;
            float y = Mathf.Sin(angle) * radiusY;

            lineRenderer.SetPosition(
                i,
                new Vector3(x, y, 0.05f)
            );
        }
    }
}
