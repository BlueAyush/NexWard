
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchLabUI : MonoBehaviour
{
    [Header("Research Data")]
    [SerializeField] private TMP_Text balanceText;

    [Header("Research Upgrades")]
    [SerializeField] private Button fieldSuppliesButton;
    [SerializeField] private TMP_Text fieldSuppliesText;

    [SerializeField] private Button reinforcedCoreButton;
    [SerializeField] private TMP_Text reinforcedCoreText;

    [SerializeField] private Button expandedConstructionButton;
    [SerializeField] private TMP_Text expandedConstructionText;

    private const string FIELD_SUPPLIES = "FieldSupplies";
    private const string REINFORCED_CORE = "ReinforcedCore";
    private const string EXPANDED_CONSTRUCTION = "ExpandedConstruction";

    private const int FIELD_SUPPLIES_COST = 25;
    private const int REINFORCED_CORE_COST = 40;
    private const int EXPANDED_CONSTRUCTION_COST = 60;

    private void Awake()
    {
        fieldSuppliesButton.onClick.AddListener(PurchaseFieldSupplies);
        reinforcedCoreButton.onClick.AddListener(PurchaseReinforcedCore);
        expandedConstructionButton.onClick.AddListener(PurchaseExpandedConstruction);
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        int total = ResearchDataManager.Total;

        balanceText.text = "RESEARCH DATA: " + total;

        UpdateUpgrade(
            fieldSuppliesButton,
            fieldSuppliesText,
            FIELD_SUPPLIES,
            FIELD_SUPPLIES_COST,
            "FIELD SUPPLIES"
        );

        UpdateUpgrade(
            reinforcedCoreButton,
            reinforcedCoreText,
            REINFORCED_CORE,
            REINFORCED_CORE_COST,
            "REINFORCED CORE"
        );

        UpdateUpgrade(
            expandedConstructionButton,
            expandedConstructionText,
            EXPANDED_CONSTRUCTION,
            EXPANDED_CONSTRUCTION_COST,
            "EXPANDED CONSTRUCTION"
        );
    }

    private void PurchaseFieldSupplies()
    {
        ResearchDataManager.TryPurchaseUnlock(
            FIELD_SUPPLIES, FIELD_SUPPLIES_COST);

        RefreshUI();
    }

    private void PurchaseReinforcedCore()
    {
        ResearchDataManager.TryPurchaseUnlock(
            REINFORCED_CORE, REINFORCED_CORE_COST);

        RefreshUI();
    }

    private void PurchaseExpandedConstruction()
    {
        ResearchDataManager.TryPurchaseUnlock(
            EXPANDED_CONSTRUCTION, EXPANDED_CONSTRUCTION_COST);

        RefreshUI();
    }

    private void UpdateUpgrade(
        Button button,
        TMP_Text label,
        string unlockID,
        int cost,
        string upgradeName)
    {
        bool unlocked = ResearchDataManager.IsUnlocked(unlockID);

        label.text = unlocked ? "UNLOCKED" : "BUY";

        button.interactable =
            !unlocked && ResearchDataManager.Total >= cost;
    }

    private void OnDestroy()
    {
        fieldSuppliesButton.onClick.RemoveListener(PurchaseFieldSupplies);
        reinforcedCoreButton.onClick.RemoveListener(PurchaseReinforcedCore);
        expandedConstructionButton.onClick.RemoveListener(PurchaseExpandedConstruction);
    }
}
