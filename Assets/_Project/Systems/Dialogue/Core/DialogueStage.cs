using System;

[Serializable]
public class DialogueStage
{
    public string stageName;

    public string dialogueID;

    public bool advanceAfterDialogue;

    public int nextStage;
}