using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonstray/Quest")]
public class QuestData : ScriptableObject
{
    public QuestRegion region = QuestRegion.Spring;

    public string questTitle;

    [TextArea(5, 20)]
    public string description;

    public List<QuestItemAmount> requiredItems = new();
    public List<NoteData> requiredNotes = new();

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

            AppendItemAmounts(builder, requiredItems);

            if (requiredNotes != null)
            {
                foreach (NoteData note in requiredNotes)
                {
                    if (note == null)
                        continue;

                    if (builder.Length > 0)
                        builder.AppendLine();

                    builder.Append("Journal: ");
                    builder.Append(note.title);
                }
            }

            return builder.Length > 0
                ? builder.ToString()
                : "Nothing required.";
        }
    }

    public string RewardText =>
        BuildRewardText();

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
}

[Serializable]
public class QuestItemAmount
{
    public ItemData item;
    public int amount = 1;
}

public enum QuestRegion
{
    Spring,
    Summer,
    Autumn,
    Winter
}
