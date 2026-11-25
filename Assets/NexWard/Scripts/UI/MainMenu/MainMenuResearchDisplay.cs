
using UnityEngine;
using TMPro;

public class MainMenuResearchDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;

    private void Update()
    {
        if (balanceText != null)
        {
            balanceText.text =
                $"RESEARCH DATA: {ResearchDataManager.Total}";
        }
    }
}
