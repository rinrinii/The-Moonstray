using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    [Tooltip("Legacy single-file dialogue source. Prefer Dialogue XML Files for new content.")]
    [SerializeField] private TextAsset dialogueXML;

    [SerializeField] private TextAsset[] dialogueXMLFiles;

    private readonly Dictionary<string, XmlNode> dialogueNodes = new();
    private readonly List<XmlDocument> loadedDocuments = new();

    private void Awake()
    {
        BuildDialogueIndex();
    }

    private void BuildDialogueIndex()
    {
        dialogueNodes.Clear();
        loadedDocuments.Clear();

        if (dialogueXML != null)
            LoadDialogueXML(dialogueXML);

        if (dialogueXMLFiles == null)
            return;

        foreach (TextAsset dialogueFile in dialogueXMLFiles)
            LoadDialogueXML(dialogueFile);
    }

    private void LoadDialogueXML(TextAsset dialogueFile)
    {
        if (dialogueFile == null)
            return;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dialogueFile.text);
        loadedDocuments.Add(xmlDoc);

        XmlNodeList nodes =
            xmlDoc.SelectNodes("/GameDialogue/Dialogue");

        if (nodes == null)
            return;

        foreach (XmlNode node in nodes)
        {
            string id = node.Attributes?["id"]?.Value;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning(
                    $"{dialogueFile.name} contains a Dialogue with no id.");
                continue;
            }

            if (dialogueNodes.ContainsKey(id))
            {
                Debug.LogWarning(
                    $"Duplicate dialogue id '{id}' found in {dialogueFile.name}. Keeping the first definition.");
                continue;
            }

            dialogueNodes.Add(id, node);
        }
    }

    public List<DialogueLine> GetDialogue(string dialogueID)
    {
        List<DialogueLine> result =
            new List<DialogueLine>();

        if (!dialogueNodes.TryGetValue(dialogueID, out XmlNode dialogueNode))
        {
            Debug.LogWarning(
                $"Dialogue not found: {dialogueID}"
            );

            return result;
        }

        foreach (XmlNode lineNode in dialogueNode.SelectNodes("Line"))
        {
            bool isNarration =
                lineNode.Attributes["type"]?.Value
                == "narration";

            Sprite portraitSprite = null;

            if (!isNarration)
            {
                string portraitName =
                    lineNode.Attributes["portrait"]?.Value ?? "";

                if (portraitName == "serin-current")
                {
                    portraitName =
                        PlayerTransformation.Instance != null &&
                        PlayerTransformation.Instance.IsWolf
                            ? "serin-wolf"
                            : "serin-human";
                }

                if (!string.IsNullOrEmpty(portraitName))
                {
                    portraitSprite =
                        Resources.Load<Sprite>(
                            $"Portraits/{portraitName}"
                        );

                    if (portraitSprite == null)
                    {
                        Debug.LogWarning(
                            $"Portrait not found: {portraitName}"
                        );
                    }
                }
            }

            DialogueLine line =
                new DialogueLine
                {
                    speaker =
                        lineNode.Attributes["speaker"]?.Value ?? "",

                    text =
                        lineNode.InnerText.Trim(),

                    portrait =
                        portraitSprite,

                    isNarration =
                        isNarration
                };

            result.Add(line);
        }

        return result;
    }
}
