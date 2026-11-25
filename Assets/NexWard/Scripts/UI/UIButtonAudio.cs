
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySelectSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIHover();
        }
    }

    private void PlaySelectSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISelect();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlaySelectSound);
        }
    }
}
