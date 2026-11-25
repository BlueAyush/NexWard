
using UnityEngine;

public class PlayerCoreInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.8f;
    [SerializeField] private float towerPriorityRange = 1.5f;

    [Header("References")]
    [SerializeField] private CoreUpgradeSystem coreUpgradeSystem;
    [SerializeField] private CoreUpgradePanel upgradePanel;

    private PlayerResources playerResources;

    private void Awake()
    {
        playerResources = GetComponent<PlayerResources>();
    }

    private void Update()
    {
        if (TowerPlacementManager.IsPlacing)
            return;

        if (PlayerUpgradePanel.IsOpen)
            return;

        if (coreUpgradeSystem == null ||
            upgradePanel == null ||
            playerResources == null)
            return;

        if (CoreUpgradePanel.IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                upgradePanel.Close();

            return;
        }

        if (TowerUpgradePanel.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Give nearby towers interaction priority.
            if (HasNearbyTower())
            {
                Debug.Log("Nearby tower detected. Tower interaction takes priority.");
                return;
            }

            float distance = Vector2.Distance(
                transform.position,
                coreUpgradeSystem.transform.position
            );

            if (distance <= interactionRange)
            {
                upgradePanel.Open(
                    coreUpgradeSystem,
                    playerResources
                );

                Debug.Log("Opened Core Upgrade Tree.");
            }
            else
            {
                Debug.Log("No interactable object nearby.");
            }
        }
    }

    private bool HasNearbyTower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            towerPriorityRange
        );

        foreach (Collider2D hit in hits)
        {
            TowerAttack tower =
                hit.GetComponentInParent<TowerAttack>();

            if (tower != null)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            towerPriorityRange
        );
    }
}
