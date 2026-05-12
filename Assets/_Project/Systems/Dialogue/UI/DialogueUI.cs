using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void Display(DialogueLine line)
    {
        speakerText.text = line.speaker;
        dialogueText.text = line.text;

        HandlePortrait(line.portrait);
    }

    private void HandlePortrait(string portraitName)
    {
        if (string.IsNullOrEmpty(portraitName))
        {
            portraitImage.gameObject.SetActive(false);
            return;
        }

        Sprite portrait = Resources.Load<Sprite>("Portraits/" + portraitName);

        if (portrait == null)
        {
            Debug.LogWarning($"Portrait not found: {portraitName}");
            portraitImage.gameObject.SetActive(false);
            return;
        }

        portraitImage.sprite = portrait;
        portraitImage.gameObject.SetActive(true);
    }
}