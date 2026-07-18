using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonstray/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Identity")]
    public string questID;
    public QuestCategory category = QuestCategory.Side;

    public QuestRegion region = QuestRegion.Spring;

    public string questTitle;

    [TextArea(5, 20)]
    public string description;

    [Header("Ordered Objectives")]
    public List<QuestObjectiveData> objectives = new();

    [Header("Map Tracking")]
    public string trackingSceneName;
    public string trackingMarkerID;

    public List<QuestRequirement> requirements = new();

    [HideInInspector] public List<QuestItemAmount> requiredItems = new();
    [HideInInspector] public List<NoteData> requiredNotes = new();

    [Header("Legacy Display Reward")]
    [TextArea(2, 8)]
    public string reward;

    [Header("Structured Rewards")]
    public int moonCoinReward;
    public List<QuestItemAmount> rewardItems = new();
    public List<NoteData> rewardNotes = new();
    public List<UpgradeType> rewardUpgrades = new();

    public string DisplayTitle =>
        string.IsNullOrWhiteSpace(questTitle)
            ? name
            : questTitle;

    public string RequiredItemsText
    {
        get
        {
            StringBuilder builder = new();

            AppendRequirements(builder, Requirements);

            return builder.Length > 0
                ? builder.ToString()
                : "Nothing required.";
        }
    }

    public string RewardText =>
        BuildRewardText();

    public IReadOnlyList<QuestRequirement> Requirements
    {
        get
        {
            if (requirements != null && requirements.Count > 0)
                return requirements;

            return BuildLegacyRequirements();
        }
    }

    public GameProgressionStage UnlockStage => region switch
    {
        QuestRegion.Spring => GameProgressionStage.Chapter1Spring,
        QuestRegion.Summer => GameProgressionStage.Chapter2Summer,
        QuestRegion.Autumn => GameProgressionStage.Chapter3Autumn,
        QuestRegion.Winter => GameProgressionStage.Chapter4Winter,
        _ => GameProgressionStage.Chapter1Spring
    };

    private string BuildRewardText()
    {
        StringBuilder builder = new();

        if (moonCoinReward > 0)
            builder.Append($"{moonCoinReward} Moon Coins");

        AppendItemAmounts(builder, rewardItems);

        if (rewardNotes != null)
        {
            foreach (NoteData note in rewardNotes)
            {
                if (note == null)
                    continue;

                if (builder.Length > 0)
                    builder.AppendLine();

                builder.Append("Journal: ");
                builder.Append(note.title);
            }
        }

        if (rewardUpgrades != null)
        {
            foreach (UpgradeType upgrade in rewardUpgrades)
            {
                if (builder.Length > 0)
                    builder.AppendLine();

                builder.Append("Upgrade: ");
                builder.Append(upgrade);
            }
        }

        if (!string.IsNullOrWhiteSpace(reward))
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(reward);
        }

        return builder.Length > 0
            ? builder.ToString()
            : "No reward listed.";
    }

    private static void AppendItemAmounts(
        StringBuilder builder,
        List<QuestItemAmount> itemAmounts)
    {
        if (itemAmounts == null)
            return;

        foreach (QuestItemAmount itemAmount in itemAmounts)
        {
            if (itemAmount == null || itemAmount.item == null)
                continue;

            if (builder.Length > 0)
                builder.AppendLine();

            int amount = Mathf.Max(1, itemAmount.amount);
            builder.Append(amount);
            builder.Append(" x ");
            builder.Append(itemAmount.item.itemName);
        }
    }

    private static void AppendRequirements(
        StringBuilder builder,
        IReadOnlyList<QuestRequirement> questRequirements)
    {
        if (questRequirements == null)
            return;

        foreach (QuestRequirement requirement in questRequirements)
        {
            if (requirement == null || !requirement.IsValid)
                continue;

            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(requirement.DisplayText);
        }
    }

    private List<QuestRequirement> BuildLegacyRequirements()
    {
        List<QuestRequirement> legacyRequirements = new();

        if (requiredItems != null)
        {
            foreach (QuestItemAmount itemAmount in requiredItems)
            {
                if (itemAmount == null || itemAmount.item == null)
                    continue;

                legacyRequirements.Add(QuestRequirement.ForItem(
                    itemAmount.item,
                    itemAmount.amount));
            }
        }

        if (requiredNotes != null)
        {
            foreach (NoteData note in requiredNotes)
            {
                if (note == null)
                    continue;

                legacyRequirements.Add(QuestRequirement.ForNote(note));
            }
        }

        return legacyRequirements;
    }

    private void OnValidate()
    {
        if (requirements == null)
            requirements = new List<QuestRequirement>();

        if (requirements.Count > 0)
            return;

        IReadOnlyList<QuestRequirement> legacyRequirements =
            BuildLegacyRequirements();

        if (legacyRequirements.Count == 0)
            return;

        requirements.AddRange(legacyRequirements);
        requiredItems?.Clear();
        requiredNotes?.Clear();
    }
}

[Serializable]
public class QuestObjectiveData
{
    public string objectiveID;

    [TextArea(2, 5)]
    public string description;

    public QuestObjectiveType type = QuestObjectiveType.Custom;
    public int requiredAmount = 1;
    public string targetID;

    [Header("Objective Tracking")]
    public ObjectiveTrackingMode trackingMode = ObjectiveTrackingMode.None;
    public string trackingMarkerID;
    public string targetScene;
    public List<string> possibleScenes = new();
    public bool hideInsideArea = true;
    [Min(0f)] public float areaRadius = 10f;

    public string FormatProgress(int currentAmount)
    {
        int required = Mathf.Max(1, requiredAmount);

        return required > 1
            ? $"{description} ({Mathf.Clamp(currentAmount, 0, required)}/{required})"
            : description;
    }

    public string PossibleAreasText =>
        possibleScenes != null && possibleScenes.Count > 0
            ? $"Possible areas: {string.Join(", ", possibleScenes)}"
            : string.Empty;
}

public enum ObjectiveTrackingMode
{
    None,
    SpecificTarget,
    SearchArea,
    SceneExit
}

[Serializable]
public class QuestItemAmount
{
    public ItemData item;
    public int amount = 1;
}

[Serializable]
public class QuestRequirement
{
    public QuestRequirementType type = QuestRequirementType.Item;
    public ItemData item;
    public NoteData note;
    public int amount = 1;

    public bool IsItem =>
        type == QuestRequirementType.Item && item != null;

    public bool IsNote =>
        type == QuestRequirementType.Note && note != null;

    public bool IsValid =>
        IsItem || IsNote;

    public string DisplayText
    {
        get
        {
            if (IsItem)
                return $"{Mathf.Max(1, amount)} x {item.itemName}";

            if (IsNote)
                return $"Journal: {note.title}";

            return string.Empty;
        }
    }

    public static QuestRequirement ForItem(ItemData item, int amount)
    {
        return new QuestRequirement
        {
            type = QuestRequirementType.Item,
            item = item,
            amount = Mathf.Max(1, amount)
        };
    }

    public static QuestRequirement ForNote(NoteData note)
    {
        return new QuestRequirement
        {
            type = QuestRequirementType.Note,
            note = note,
            amount = 1
        };
    }
}

public enum QuestRequirementType
{
    Item,
    Note
}

public enum QuestRegion
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum QuestCategory
{
    Main,
    Side,
    Tutorial
}

public enum QuestObjectiveType
{
    Custom,
    Travel,
    Interact,
    Collect,
    ReadNote,
    Transform,
    Dialogue
}
