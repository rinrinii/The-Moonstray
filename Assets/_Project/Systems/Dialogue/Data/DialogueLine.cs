using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;

    public Sprite portrait;

    public bool isNarration;

    public List<string> choices;

    public bool HasChoices => choices != null && choices.Count > 0;
}
