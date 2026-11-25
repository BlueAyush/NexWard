
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradeNodeUI : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private TowerAttack.UpgradePath path;
    [SerializeField, Range(1, 3)] private int tier = 1;
    [SerializeField] private string upgradeName;

    private Button button;
    private TMP_Text buttonText;
    private TowerUpgradePanel owner;

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(TowerUpgradePanel panel)
    {
        CacheComponents();

        owner = panel;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (owner != null)
            owner.PurchaseUpgrade(path, tier);
    }

    public void Refresh(
        TowerAttack tower,
        PlayerResources resources)
    {
        if (tower == null || resources == null)
            return;

        int currentTier = tower.GetUpgradeTier(path);

        bool pathLocked = tower.IsPathLocked(path);
        bool tier3Locked =
            tier == 3 && tower.IsTier3LockedForPath(path);

        bool alreadyOwned = tier <= currentTier;

        bool previousTierMissing =
            tier > currentTier + 1;

        bool canPurchase =
            !pathLocked &&
            !alreadyOwned &&
            !previousTierMissing &&
            tower.CanPurchaseUpgrade(path, tier) &&
            resources.CommandPoints >= 1f;

        if (button != null)
            button.interactable = canPurchase;

        if (buttonText == null)
            return;

        string displayName =
            tower.GetUpgradeDisplayName(path, tier);

        if (alreadyOwned)
        {
            buttonText.text = $"T{tier}\n{displayName}\nOWNED";
        }
        else if (pathLocked)
        {
            buttonText.text = $"T{tier}\n{displayName}\nPATH LOCKED";
        }
        else if (tier3Locked)
        {
            buttonText.text = $"T{tier}\n{displayName}\nTIER 3 USED";
        }
        else if (previousTierMissing)
        {
            buttonText.text = $"T{tier}\n{displayName}\nBUY PREVIOUS";
        }
        else if (resources.CommandPoints < 1f)
        {
            buttonText.text = $"T{tier}\n{displayName}\nNEED 1 CP";
        }
        else
        {
            buttonText.text = $"T{tier}\n{displayName}\n1 CP";
        }
    }
}
