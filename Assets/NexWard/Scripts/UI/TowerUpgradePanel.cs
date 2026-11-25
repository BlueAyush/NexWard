
using UnityEngine;
using TMPro;

public class TowerUpgradePanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text commandPointsText;

    [Header("Upgrade Path Headers")]
    [SerializeField] private TMP_Text powerHeader;
    [SerializeField] private TMP_Text speedHeader;
    [SerializeField] private TMP_Text rangeHeader;

    [Header("Repair")]
    [SerializeField] private UnityEngine.UI.Button repairButton;
    [SerializeField] private int repairScrapCost = 5;
    [SerializeField] private float repairAmount = 20f;

    private TowerAttack selectedTower;
    private PlayerResources playerResources;

    private TowerUpgradeNodeUI[] upgradeNodes;

    public TowerAttack SelectedTower => selectedTower;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        IsOpen = false;
    }

    public void Open(
        TowerAttack tower,
        PlayerResources resources)
    {
        if (tower == null || resources == null || panelRoot == null)
            return;

        selectedTower = tower;
        playerResources = resources;

        panelRoot.SetActive(true);
        IsOpen = true;

        TowerRangeIndicator rangeIndicator =
            selectedTower.GetComponent<TowerRangeIndicator>();

        if (rangeIndicator != null)
        {
            rangeIndicator.Show(true);
        }

        upgradeNodes =
            panelRoot.GetComponentsInChildren<TowerUpgradeNodeUI>(true);

        foreach (TowerUpgradeNodeUI node in upgradeNodes)
        {
            node.Initialize(this);
        }

        Refresh();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (selectedTower != null)
        {
            TowerRangeIndicator rangeIndicator =
                selectedTower.GetComponent<TowerRangeIndicator>();

            if (rangeIndicator != null)
                rangeIndicator.Show(false);
        }
        selectedTower = null;
        playerResources = null;

        IsOpen = false;
    }

    public void PurchaseUpgrade(
        TowerAttack.UpgradePath path,
        int tier)
    {
        if (selectedTower == null || playerResources == null)
            return;

        if (!selectedTower.CanPurchaseUpgrade(path, tier))
            return;

        if (!playerResources.SpendCommandPoint())
            return;

        selectedTower.TryPurchaseUpgrade(path, tier);

        Debug.Log($"Purchased {path} Tier {tier}");

        Refresh();
    }

    public void RepairSelectedTower()
    {
        if (selectedTower == null || playerResources == null)
            return;

        TowerHealth towerHealth =
            selectedTower.GetComponent<TowerHealth>();

        if (towerHealth == null)
            return;

        if (towerHealth.IsFullyRepaired)
        {
            Debug.Log("Tower is already at full health.");
            return;
        }

        if (!playerResources.SpendScrap(repairScrapCost))
        {
            Debug.Log("Not enough Scrap to repair this tower!");
            return;
        }

        towerHealth.Repair(repairAmount);

        Refresh();

        Debug.Log("Tower repair completed!");
    }

    public void DemolishSelectedTower()
    {
        if (selectedTower == null || playerResources == null)
            return;

        TowerHealth towerHealth =
            selectedTower.GetComponent<TowerHealth>();

        if (towerHealth == null)
            return;

        PlayerResources resources = playerResources;

        // Close the panel and hide the range indicator.
        Close();

        // Refund resources and remove the tower.
        towerHealth.Demolish(resources);
    }

    private void Refresh()
    {
        if (selectedTower == null || playerResources == null)
            return;

        TowerRangeIndicator rangeIndicator =
        selectedTower.GetComponent<TowerRangeIndicator>();

        if (rangeIndicator != null)
            rangeIndicator.Refresh();

        titleText.text = selectedTower.GetTowerDisplayName();

        if (powerHeader != null)
        {
            powerHeader.text =
                selectedTower.GetPathDisplayName(
                    TowerAttack.UpgradePath.Power
                );
        }

        if (speedHeader != null)
        {
            speedHeader.text =
                selectedTower.GetPathDisplayName(
                    TowerAttack.UpgradePath.Speed
                );
        }

        if (rangeHeader != null)
        {
            rangeHeader.text =
                selectedTower.GetPathDisplayName(
                    TowerAttack.UpgradePath.Range
                );
        }

        if (selectedTower.Type == TowerAttack.TowerType.Tesla)
        {
            statsText.text =
                $"Arc Damage: {selectedTower.Damage:F1}\n" +
                $"Discharge: {selectedTower.FireInterval:F2}s\n" +
                $"Attack Range: {selectedTower.AttackRange:F1}";
        }
        else
        {
            statsText.text =
                $"Damage: {selectedTower.Damage:F1}\n" +
                $"Fire Interval: {selectedTower.FireInterval:F2}s\n" +
                $"Attack Range: {selectedTower.AttackRange:F1}";
        }

        commandPointsText.text =
            $"COMMAND POINTS: {playerResources.CommandPoints:0.##}";

        if (upgradeNodes == null)
            return;

        foreach (TowerUpgradeNodeUI node in upgradeNodes)
        {
            node.Refresh(selectedTower, playerResources);
        }

        TowerHealth towerHealth =
            selectedTower.GetComponent<TowerHealth>();

        if (towerHealth != null)
        {
            statsText.text +=
                $"\nHealth: {towerHealth.CurrentHealth:F0}/" +
                $"{towerHealth.MaxHealth:F0}";

            if (repairButton != null)
            {
                repairButton.interactable =
                    !towerHealth.IsFullyRepaired &&
                    playerResources.Scrap >= repairScrapCost;
            }
        }
    }

    private void Update()
    {
        if (IsOpen && selectedTower == null)
            Close();
    }
}
