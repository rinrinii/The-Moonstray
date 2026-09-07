using UnityEngine;
using UnityEngine.UIElements;
using System.Xml;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Mapping")]
    [SerializeField] private string gamePlayScene = "Pinewatch Trail";

    [SerializeField] private SessionInitializer sessionInitializer;

    private UIDocument uiDocument;

    private VisualElement mainMenuPanel;
    private VisualElement settingsPanel;
    private VisualElement creditsPanel;
    private VisualElement specialThanksContainer;

    private SettingsController settingsController;

    private Button continueBtn;
    private Button newGameBtn;
    private Button settingsBtn;
    private Button creditsBtn;
    private Button creditsCloseBtn;
    private Button exitBtn;
    private Button backBtn;
    private VisualElement saveSlotOverlay;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        settingsController = GetComponent<SettingsController>();
        AssignUIReferences();

        // Music and other cross-scene services must exist while the Main Menu
        // is open, not only after New Game or Continue is pressed.
        sessionInitializer?.CreateSession();
    }

    private void OnEnable()
    {
        if (continueBtn != null) continueBtn.clicked += OnContinuePressed;
        if (newGameBtn != null) newGameBtn.clicked += OnNewGamePressed;
        if (settingsBtn != null) settingsBtn.clicked += OpenSettings;
        if (creditsBtn != null) creditsBtn.clicked += OpenCredits;
        if (creditsCloseBtn != null) creditsCloseBtn.clicked += CloseCredits;
        if (exitBtn != null) exitBtn.clicked += CloseGame;
        if (backBtn != null) backBtn.clicked += CloseSettings;

        RefreshContinueButton();
    }

    private void OnDisable()
    {
        if (continueBtn != null) continueBtn.clicked -= OnContinuePressed;
        if (newGameBtn != null) newGameBtn.clicked -= OnNewGamePressed;
        if (settingsBtn != null) settingsBtn.clicked -= OpenSettings;
        if (creditsBtn != null) creditsBtn.clicked -= OpenCredits;
        if (creditsCloseBtn != null) creditsCloseBtn.clicked -= CloseCredits;
        if (exitBtn != null) exitBtn.clicked -= CloseGame;
        if (backBtn != null) backBtn.clicked -= CloseSettings;
    }

    private void AssignUIReferences()
    {
        VisualElement root = uiDocument.rootVisualElement;

        mainMenuPanel = root.Q<VisualElement>("MainMenu");
        settingsPanel = root.Q<VisualElement>("SettingsRoot");

        VisualTreeAsset creditsTemplate = Resources.Load<VisualTreeAsset>("UI/CreditsTemplate");
        creditsTemplate?.CloneTree(root);
        creditsPanel = root.Q<VisualElement>("CreditsRoot");
        specialThanksContainer = root.Q<VisualElement>("SpecialThanksNames");

        continueBtn = root.Q<Button>("ContinueButton");
        newGameBtn = root.Q<Button>("NewGameButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        creditsBtn = root.Q<Button>("CreditsButton");
        creditsCloseBtn = root.Q<Button>("CreditsCloseButton");
        exitBtn = root.Q<Button>("ExitButton");

        backBtn = root.Q<Button>("BackButton");

        if (backBtn == null)
            backBtn = root.Q<Button>("Back-Button");

        PopulateSpecialThanks();
    }

    private void OnContinuePressed()
    {
        AudioManager.Instance?.PlayUI("Button3");

        OpenLoadSlots();
    }

    private void LoadSlot(int slot)
    {

        sessionInitializer?.CreateSession();

        if (!SaveGameService.TryLoad(slot, out GameSessionManager.Snapshot snapshot, out string error))
        {
            Debug.LogWarning($"Continue failed: {error}");
            RefreshContinueButton();
            return;
        }

        GameSessionManager.RestoreState(snapshot);
    }

    private void OnNewGamePressed()
    {
        AudioManager.Instance?.PlayUI("Button3");

        Debug.Log("MAIN MENU: New Game");

        sessionInitializer.CreateSession();
        GameSessionManager.ResetProgressForNewGame();

        Debug.Log("TutorialManager = " + TutorialManager.Instance);

        TutorialManager.Instance.StartTutorial();

        PlayerTransformation.Instance?.ForceWolfForm();
        PlayerTransformation.Instance?.LockTransformation();
        PlayerTransformation.Instance?.HoldWolfRestPose();

        SceneLoader.LoadScene(
            gamePlayScene,
            "ToPinewatchTrail"
        );
    }

    private void OpenSettings()
    {
        AudioManager.Instance?.PlayUI("Button3");

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.None;

        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.Flex;
        else
            Debug.LogError("SettingsRoot not found.");
    }

    private void CloseSettings()
    {
        AudioManager.Instance?.PlayUI("Button3");

        settingsController?.RevertToSavedSettings();

        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.None;

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseGame()
    {
        AudioManager.Instance?.PlayUI("Button3");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadGameplayScene()
    {
        SceneLoader.LoadScene(gamePlayScene);
    }

    private void OpenCredits()
    {
        AudioManager.Instance?.PlayUI("Button3");
        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.None;
        if (creditsPanel != null)
            creditsPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseCredits()
    {
        AudioManager.Instance?.PlayUI("Button3");
        if (creditsPanel != null)
            creditsPanel.style.display = DisplayStyle.None;
        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    private void PopulateSpecialThanks()
    {
        if (specialThanksContainer == null)
            return;

        specialThanksContainer.Clear();
        TextAsset creditsData = Resources.Load<TextAsset>("Data/Credits");
        if (creditsData == null)
            return;

        try
        {
            XmlDocument document = new();
            document.LoadXml(creditsData.text);
            XmlNodeList people = document.SelectNodes("/credits/specialThanks/person");
            if (people == null)
                return;

            foreach (XmlNode person in people)
            {
                string personName = person.Attributes?["name"]?.Value;
                if (!string.IsNullOrWhiteSpace(personName))
                {
                    Label nameLabel = new(personName);
                    nameLabel.AddToClassList("credits-special-thanks-name");
                    specialThanksContainer.Add(nameLabel);
                }
            }
        }
        catch (XmlException exception)
        {
            Debug.LogWarning($"Credits XML could not be read: {exception.Message}");
        }
    }

    private void RefreshContinueButton()
    {
        bool hasSave = SaveGameService.HasAnyValidSave();
        continueBtn?.SetEnabled(hasSave);
        if (continueBtn != null)
            continueBtn.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OpenLoadSlots()
    {
        CloseSaveSlots();
        saveSlotOverlay = SaveSlotPanel.Open(uiDocument.rootVisualElement, "CONTINUE", false, LoadSlot, CloseSaveSlots);
    }

    private void CloseSaveSlots()
    {
        SaveSlotPanel.Close(saveSlotOverlay);
        saveSlotOverlay = null;
    }
}
