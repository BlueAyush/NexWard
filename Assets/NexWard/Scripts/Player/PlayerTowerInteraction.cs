
using UnityEngine;

public class PlayerTowerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private TowerUpgradePanel upgradePanel;

    private PlayerResources playerResources;

    private void Awake()
    {
        playerResources = GetComponent<PlayerResources>();

        if (playerResources == null)
            Debug.LogError("PlayerResources missing from Player!");

        if (upgradePanel == null)
            Debug.LogError("TowerUpgradePanel reference is missing!");
    }

    private void Update()
    {
        if (TowerPlacementManager.IsPlacing)
            return;

        if (PlayerUpgradePanel.IsOpen ||
            CoreUpgradePanel.IsOpen)
            return;

        if (upgradePanel == null || playerResources == null)
            return;

        if (CoreUpgradePanel.IsOpen)
            return;

        // Keep the panel open until explicitly closed.
        if (TowerUpgradePanel.IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
                upgradePanel.Close();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed. Searching for tower...");

            TowerAttack tower = FindNearbyTower();

            if (tower != null)
            {
                Debug.Log("Found tower: " + tower.name);

                upgradePanel.Open(tower, playerResources);
            }
            else
            {
                Debug.Log("No tower found within interaction range.");
            }
        }
    }

    private TowerAttack FindNearbyTower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRange
        );

        TowerAttack closestTower = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            TowerAttack tower =
                hit.GetComponentInParent<TowerAttack>();

            if (tower == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                tower.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = tower;
            }
        }

        return closestTower;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}
