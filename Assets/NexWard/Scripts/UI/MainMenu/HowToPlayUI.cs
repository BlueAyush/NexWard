
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("Instruction Images")]
    [SerializeField] private Image instructionImage;
    [SerializeField] private Sprite[] instructionPages;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Page Indicators")]
    [SerializeField] private Image[] pageIndicators;
    [SerializeField] private Color activeDotColor = Color.white;
    [SerializeField] private Color inactiveDotColor =
        new Color(0.4f, 0.4f, 0.4f, 0.5f);

    private int currentPage;

    private void Awake()
    {
        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
    }

    private void Start()
    {
        currentPage = 0;
        RefreshPage();
    }

    private void PreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        RefreshPage();
    }

    private void NextPage()
    {
        if (currentPage >= instructionPages.Length - 1)
            return;

        currentPage++;
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (instructionPages == null ||
            instructionPages.Length == 0)
        {
            Debug.LogWarning("HowToPlayUI: No instruction images assigned.");
            return;
        }

        currentPage = Mathf.Clamp(
            currentPage,
            0,
            instructionPages.Length - 1
        );

        instructionImage.sprite = instructionPages[currentPage];

        previousButton.interactable = currentPage > 0;
        nextButton.interactable =
            currentPage < instructionPages.Length - 1;

        for (int i = 0; i < pageIndicators.Length; i++)
        {
            if (pageIndicators[i] == null)
                continue;

            pageIndicators[i].color =
                i == currentPage ? activeDotColor : inactiveDotColor;
        }
    }

    private void OnDestroy()
    {
        previousButton.onClick.RemoveListener(PreviousPage);
        nextButton.onClick.RemoveListener(NextPage);
    }
}
