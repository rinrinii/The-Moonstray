using System;

[Serializable]
public class DialogueStage
{
    public string stageName;

    public string dialogueID;

    public bool advanceAfterDialogue;

    public int nextStage;

    public string progressionFlagOnComplete;

    public string objectiveTitleOnComplete;

    public string objectiveDescriptionOnComplete;
}
