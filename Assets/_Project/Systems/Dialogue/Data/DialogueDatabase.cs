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

        XmlNode dialogueNode = xmlDoc.SelectSingleNode(
            $"/GameDialogue/Dialogue[@id='{dialogueID}']"
        );

        if (dialogueNode == null)
        {
            Debug.LogWarning($"Dialogue not found: {dialogueID}");
            return result;
        }

        foreach (XmlNode lineNode in dialogueNode.SelectNodes("Line"))
        {
            DialogueLine line = new DialogueLine
            {
                speaker = lineNode.Attributes["speaker"]?.Value ?? "",
                text = lineNode.InnerText.Trim(),
                portrait = lineNode.Attributes["portrait"]?.Value ?? ""
            };

            result.Add(line);
        }

        return result;
    }
}