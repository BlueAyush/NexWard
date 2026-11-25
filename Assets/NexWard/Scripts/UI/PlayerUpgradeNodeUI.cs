
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUpgradeNodeUI : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private PlayerUpgradeSystem.UpgradePath path;
    [SerializeField, Range(1, 3)] private int tier = 1;
    [SerializeField] private string upgradeName;

    private Button button;
    private TMP_Text buttonText;
    private PlayerUpgradePanel owner;

    private void CacheComponents()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(PlayerUpgradePanel panel)
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
        PlayerUpgradeSystem player,
        PlayerResources resources)
    {
        if (player == null || resources == null)
            return;

        int currentTier = player.GetUpgradeTier(path);

        bool pathLocked = player.IsPathLocked(path);

        bool tier3Locked =
            tier == 3 && player.IsTier3LockedForPath(path);

        bool alreadyOwned = tier <= currentTier;

        bool previousTierMissing =
            tier > currentTier + 1;

        bool canPurchase =
            player.CanPurchaseUpgrade(path, tier) &&
            resources.CommandPoints >= 1f;

        if (button != null)
            button.interactable = canPurchase;

        if (buttonText == null)
            return;

        if (alreadyOwned)
            buttonText.text = $"T{tier}\n{upgradeName}\nOWNED";
        else if (pathLocked)
            buttonText.text = $"T{tier}\n{upgradeName}\nPATH LOCKED";
        else if (tier3Locked)
            buttonText.text = $"T{tier}\n{upgradeName}\nTIER 3 USED";
        else if (previousTierMissing)
            buttonText.text = $"T{tier}\n{upgradeName}\nBUY PREVIOUS";
        else if (resources.CommandPoints <= 0)
            buttonText.text = $"T{tier}\n{upgradeName}\nNEED 1 CP";
        else
            buttonText.text = $"T{tier}\n{upgradeName}\n1 CP";
    }
}
