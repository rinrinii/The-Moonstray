using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    [SerializeField] private TextAsset dialogueXML;

    private XmlDocument xmlDoc;

    private void Awake()
    {
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dialogueXML.text);
    }

    public List<DialogueLine> GetDialogue(string dialogueID)
    {
        List<DialogueLine> result = new List<DialogueLine>();

        string[] parts = dialogueID.Split('.');
        if (parts.Length != 2)
        {
            Debug.LogWarning("Invalid dialogue ID format.");
            return result;
        }

        string chapterID = parts[0];
        string partID = parts[1];

        XmlNode node = xmlDoc.SelectSingleNode(
            $"/dialogues/chapter[@id='{chapterID}']/part[@id='{partID}']"
        );

        if (node == null)
        {
            Debug.LogWarning($"Dialogue not found: {dialogueID}");
            return result;
        }

        foreach (XmlNode lineNode in node.SelectNodes("line"))
        {
            DialogueLine line = new DialogueLine
            {
                speaker = lineNode.Attributes["speaker"]?.Value,
                text = lineNode.InnerText,
                portrait = lineNode.Attributes["portrait"]?.Value
            };

            result.Add(line);
        }

        return result;
    }
}