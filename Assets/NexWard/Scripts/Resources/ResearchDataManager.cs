
using UnityEngine;

public static class ResearchDataManager
{
    private const string ResearchDataKey =
        "Nexward_ResearchData";

    private const string UnlockKeyPrefix =
        "Nexward_Unlock_";

    public static int Total =>
        PlayerPrefs.GetInt(ResearchDataKey, 0);

    public static void Add(int amount)
    {
        if (amount <= 0)
            return;

        int newTotal = Total + amount;

        PlayerPrefs.SetInt(ResearchDataKey, newTotal);
        PlayerPrefs.Save();

        Debug.Log(
            $"Research Data earned: +{amount}. Total: {newTotal}"
        );
    }

    public static bool TrySpend(int amount)
    {
        if (amount <= 0 || Total < amount)
            return false;

        PlayerPrefs.SetInt(
            ResearchDataKey,
            Total - amount
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"Research Data spent: {amount}. Remaining: {Total}"
        );

        return true;
    }

    public static bool IsUnlocked(string unlockID)
    {
        return PlayerPrefs.GetInt(
            UnlockKeyPrefix + unlockID,
            0
        ) == 1;
    }

    public static bool TryPurchaseUnlock(
        string unlockID,
        int cost)
    {
        if (string.IsNullOrEmpty(unlockID) ||
            cost <= 0 ||
            IsUnlocked(unlockID))
        {
            return false;
        }

        if (!TrySpend(cost))
            return false;

        PlayerPrefs.SetInt(
            UnlockKeyPrefix + unlockID,
            1
        );

        PlayerPrefs.Save();

        Debug.Log($"Permanent unlock purchased: {unlockID}");

        return true;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Nexward/Reset Research Progress")]
    private static void ResetResearchProgress()
    {
        if (!UnityEditor.EditorUtility.DisplayDialog(
            "Reset Research Progress",
            "This will reset all Research Data and permanent Lab upgrades. Continue?",
            "Reset",
            "Cancel"))
            return;

        PlayerPrefs.DeleteKey("Nexward_ResearchData");

        PlayerPrefs.DeleteKey("Nexward_Unlock_FieldSupplies");
        PlayerPrefs.DeleteKey("Nexward_Unlock_ReinforcedCore");
        PlayerPrefs.DeleteKey("Nexward_Unlock_ExpandedConstruction");

        PlayerPrefs.Save();

        Debug.Log("Research progress reset!");
    }
#endif
}
