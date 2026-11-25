
using UnityEngine;

public class PlayerUpgradeInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUpgradePanel upgradePanel;

    private PlayerUpgradeSystem playerUpgradeSystem;
    private PlayerResources playerResources;

    private void Awake()
    {
        playerUpgradeSystem = GetComponent<PlayerUpgradeSystem>();
        playerResources = GetComponent<PlayerResources>();
    }

    private void Update()
    {
        if (TowerPlacementManager.IsPlacing)
            return;

        if (upgradePanel == null ||
            playerUpgradeSystem == null ||
            playerResources == null)
            return;

        if (PlayerUpgradePanel.IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.R))
                upgradePanel.Close();

            return;
        }

        if (TowerUpgradePanel.IsOpen ||
            CoreUpgradePanel.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            upgradePanel.Open(
                playerUpgradeSystem,
                playerResources
            );

            Debug.Log("Opened Player Upgrade Tree.");
        }
    }
}
