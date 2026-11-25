
using UnityEngine;
using TMPro;

public class CoreUpgradePanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text commandPointsText;

    private CoreUpgradeSystem selectedCore;
    private PlayerResources playerResources;

    private CoreUpgradeNodeUI[] upgradeNodes;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        IsOpen = false;
    }

    public void Open(
        CoreUpgradeSystem core,
        PlayerResources resources)
    {
        if (core == null || resources == null || panelRoot == null)
            return;

        if (TowerUpgradePanel.IsOpen)
            return;

        selectedCore = core;
        playerResources = resources;

        panelRoot.SetActive(true);
        IsOpen = true;

        upgradeNodes =
            panelRoot.GetComponentsInChildren<CoreUpgradeNodeUI>(true);

        foreach (CoreUpgradeNodeUI node in upgradeNodes)
        {
            node.Initialize(this);
        }

        Refresh();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        selectedCore = null;
        playerResources = null;

        IsOpen = false;
    }

    public void PurchaseUpgrade(
        CoreUpgradeSystem.UpgradePath path,
        int tier)
    {
        if (selectedCore == null || playerResources == null)
            return;

        if (!selectedCore.CanPurchaseUpgrade(path, tier))
            return;

        if (playerResources.CommandPoints < 1f)
            return;

        if (!selectedCore.TryPurchaseUpgrade(path, tier))
            return;

        playerResources.SpendCommandPoint();

        Refresh();
    }

    private void Refresh()
    {
        if (selectedCore == null || playerResources == null)
            return;

        CoreHealth coreHealth = selectedCore.Core;
        CoreDome dome = selectedCore.Dome;

        if (titleText != null)
            titleText.text = "CORE UPGRADE SYSTEM";

        if (statsText != null)
        {
            statsText.text =
                $"Core Health: {coreHealth.CurrentHealth:F0}/{coreHealth.MaxHealth}\n" +
                $"Dome Radius: {dome.Radius:F1}\n" +
                $"Tower Damage Reduction: {dome.TowerDamageReduction:P0}\n" +
                $"Regeneration: {selectedCore.RegenerationPerSecond:F2} HP/s\n" +
                $"Pulse: {(selectedCore.PulseUnlocked ? "ACTIVE" : "LOCKED")}";
        }

        if (commandPointsText != null)
        {
            commandPointsText.text =
                $"COMMAND POINTS: {playerResources.CommandPoints:0.##}";
        }

        if (upgradeNodes == null)
            return;

        foreach (CoreUpgradeNodeUI node in upgradeNodes)
        {
            node.Refresh(selectedCore, playerResources);
        }
    }
}
