
using UnityEngine;
using TMPro;

public class PlayerUpgradePanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text commandPointsText;

    private PlayerUpgradeSystem selectedPlayer;
    private PlayerResources playerResources;

    private PlayerUpgradeNodeUI[] upgradeNodes;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        IsOpen = false;
    }

    public void Open(
        PlayerUpgradeSystem player,
        PlayerResources resources)
    {
        if (player == null || resources == null || panelRoot == null)
            return;

        if (TowerUpgradePanel.IsOpen ||
            CoreUpgradePanel.IsOpen)
            return;

        selectedPlayer = player;
        playerResources = resources;

        panelRoot.SetActive(true);
        IsOpen = true;

        upgradeNodes =
            panelRoot.GetComponentsInChildren<PlayerUpgradeNodeUI>(true);

        foreach (PlayerUpgradeNodeUI node in upgradeNodes)
            node.Initialize(this);

        Refresh();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        selectedPlayer = null;
        playerResources = null;

        IsOpen = false;
    }

    public void PurchaseUpgrade(
        PlayerUpgradeSystem.UpgradePath path,
        int tier)
    {
        if (selectedPlayer == null || playerResources == null)
            return;

        if (playerResources.CommandPoints < 1f)
            return;

        if (!selectedPlayer.CanPurchaseUpgrade(path, tier))
            return;

        if (!selectedPlayer.TryPurchaseUpgrade(path, tier))
            return;

        playerResources.SpendCommandPoint();

        Refresh();
    }

    private void Refresh()
    {
        if (selectedPlayer == null || playerResources == null)
            return;

        if (titleText != null)
            titleText.text = "PLAYER UPGRADE SYSTEM";

        if (statsText != null)
        {
            PlayerHealth health = selectedPlayer.Health;
            PlayerWeapon weapon = selectedPlayer.Weapon;
            PlayerMovement movement = selectedPlayer.Movement;

            statsText.text =
                $"Health: {health.CurrentHealth:F0}/{health.MaxHealth:F0}\n" +
                $"Damage: {weapon.Damage:F1}\n" +
                $"Movement Speed: {movement.CurrentMoveSpeed:F1}\n" +
                $"Damage Resistance: {health.DamageReduction:P0}\n" +
                $"Dash: {(movement.DashUnlocked ? "UNLOCKED" : "LOCKED")}";
        }

        if (commandPointsText != null)
        {
            commandPointsText.text =
                $"COMMAND POINTS: {playerResources.CommandPoints:0.##}";
        }

        if (upgradeNodes == null)
            return;

        foreach (PlayerUpgradeNodeUI node in upgradeNodes)
            node.Refresh(selectedPlayer, playerResources);
    }
}
