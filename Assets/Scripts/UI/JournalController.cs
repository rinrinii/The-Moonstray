using UnityEngine;
using UnityEngine.UIElements;

public class JournalController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement journalContainer;
    private bool isJournalOpen = false;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        journalContainer = root.Q<VisualElement>("JournalContainer");

        if (journalContainer != null)
        {
            journalContainer.style.display = DisplayStyle.None;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }
    }

    private void ToggleJournal()
    {
        if (journalContainer == null) return;

        isJournalOpen = !isJournalOpen;
        journalContainer.style.display = isJournalOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }
}