
/*
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject uiParent; // The container for your dialogue UI
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Settings")]
    public float typingSpeed = 0.04f;

    private XmlDocument xmlDoc;
    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;

    struct DialogueLine { public string name; public string text; public string portrait; }

    void Awake() { 
        Instance = this; 
        if (uiParent != null) uiParent.SetActive(false); 
    }

    public void StartDialogue(string chapterId, string partId) {
        if (dialogueActive) return;

        if (xmlDoc == null) {
            TextAsset xmlData = Resources.Load<TextAsset>("DialogueData");
            if (xmlData == null) { Debug.LogError("XML Not Found!"); return; }
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData.text);
        }

        lines.Clear();
        string query = $"//Chapter[@id='{chapterId}']/Part[@id='{partId}']/Line";
        XmlNodeList nodeList = xmlDoc.SelectNodes(query);

        foreach (XmlNode node in nodeList) {
            lines.Enqueue(new DialogueLine {
                name = node.Attributes["speaker"]?.Value ?? "",
                portrait = node.Attributes["portrait"]?.Value ?? "",
                text = node.InnerText
            });
        }

        dialogueActive = true;
        uiParent.SetActive(true); // Instant Show
        DisplayNextLine();
    }

    public void DisplayNextLine() {
        if (lines.Count == 0) { 
            EndDialogue(); 
            return; 
        }

        DialogueLine currentLine = lines.Dequeue();
        
        speakerText.text = currentLine.name;
        speakerText.gameObject.SetActive(!string.IsNullOrEmpty(currentLine.name));

        // Handle Portrait
        if (!string.IsNullOrEmpty(currentLine.portrait)) {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = Resources.Load<Sprite>("Portraits/" + currentLine.portrait);
        } else {
            portraitImage.gameObject.SetActive(false);
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentLine.text));
    }

    IEnumerator TypeSentence(string sentence) {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void Update() {
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space) && !isTyping) {
            DisplayNextLine();
        }
    }

    private void EndDialogue() {
        dialogueActive = false;
        uiParent.SetActive(false); // Instant Hide
    }
}

*/