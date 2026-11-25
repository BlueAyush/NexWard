
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoreUpgradeNodeUI : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private CoreUpgradeSystem.UpgradePath path;
    [SerializeField, Range(1, 3)] private int tier = 1;
    [SerializeField] private string upgradeName;

    private Button button;
    private TMP_Text buttonText;
    private CoreUpgradePanel owner;

    private void CacheComponents()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(CoreUpgradePanel panel)
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
        CoreUpgradeSystem core,
        PlayerResources resources)
    {
        if (core == null || resources == null)
            return;

        int currentTier = core.GetUpgradeTier(path);

        bool pathLocked = core.IsPathLocked(path);

        bool tier3Locked =
            tier == 3 && core.IsTier3LockedForPath(path);

        bool alreadyOwned = tier <= currentTier;

        bool previousTierMissing =
            tier > currentTier + 1;

        bool canPurchase =
            core.CanPurchaseUpgrade(path, tier) &&
            resources.CommandPoints >= 1f;

        if (button != null)
            button.interactable = canPurchase;

        if (buttonText == null)
            return;

        if (alreadyOwned)
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\nOWNED";
        }
        else if (pathLocked)
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\nPATH LOCKED";
        }
        else if (tier3Locked)
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\nTIER 3 USED";
        }
        else if (previousTierMissing)
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\nBUY PREVIOUS TIER";
        }
        else if (resources.CommandPoints <= 0)
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\nNEED 1 CP";
        }
        else
        {
            buttonText.text =
                $"TIER {tier}\n{upgradeName}\n1 CP";
        }
    }
}
