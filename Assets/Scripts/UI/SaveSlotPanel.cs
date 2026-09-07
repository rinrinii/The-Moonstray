using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public static class SaveSlotPanel
{
    public static VisualElement Open(VisualElement root, string title, bool allowEmpty, Action<int> select, Action close, bool confirmOccupied = false)
    {
        VisualElement overlay = root.Q<VisualElement>("LoadSaveRoot");
        if (overlay == null)
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UI/SaveTemplate");
            template?.CloneTree(root);
            overlay = root.Q<VisualElement>("LoadSaveRoot");
        }
        VisualElement container = overlay?.Q<VisualElement>("SaveStatesContainer");
        if (overlay == null || container == null)
            return null;

        overlay.Q<Label>("LoadSave-Label").text = title;
        container.Clear();

        for (int slot = 0; slot < SaveGameService.SlotCount; slot++)
        {
            int selectedSlot = slot;
            bool exists = SaveGameService.TryGetSlotInfo(slot, out SaveGameService.SlotInfo info);
            Button button = new(() =>
            {
                if (confirmOccupied && exists)
                {
                    ShowOverwriteConfirmation(overlay, selectedSlot, select);
                    return;
                }

                select(selectedSlot);
            });
            button.AddToClassList("save-slot");
            button.SetEnabled(allowEmpty || exists);

            Image image = new();
            image.AddToClassList("save-slot-thumbnail");
            Texture2D thumbnail = LoadThumbnail(slot);
            if (thumbnail != null) image.image = thumbnail;
            button.Add(image);

            VisualElement details = new();
            details.AddToClassList("save-slot-details");
            Label slotTitle = new($"SLOT {slot + 1}");
            slotTitle.AddToClassList("save-slot-title");
            details.Add(slotTitle);
            details.Add(new Label(exists ? info.Location : "Empty Slot"));
            if (exists)
            {
                details.Add(new Label(info.MainQuest));
                Label time = new($"Last Saved: {info.SavedAt}");
                time.AddToClassList("save-slot-time");
                details.Add(time);
            }
            button.Add(details);
            container.Add(button);
        }

        Button back = overlay.Q<Button>("SaveSlotsBackButton");
        back.clicked -= close;
        back.clicked += close;
        overlay.style.display = DisplayStyle.Flex;
        overlay.BringToFront();
        return overlay;
    }

    private static void ShowOverwriteConfirmation(
        VisualElement overlay,
        int slot,
        Action<int> confirm)
    {
        VisualElement existing = overlay.Q<VisualElement>("OverwriteConfirmation");
        existing?.RemoveFromHierarchy();

        VisualElement blocker = new() { name = "OverwriteConfirmation" };
        blocker.AddToClassList("save-confirmation-blocker");

        VisualElement dialog = new();
        dialog.AddToClassList("save-confirmation-dialog");

        Label title = new("OVERWRITE SAVE?");
        title.AddToClassList("save-confirmation-title");
        dialog.Add(title);
        dialog.Add(new Label($"Slot {slot + 1} already contains a save.\nOverwrite this save slot?"));

        VisualElement actions = new();
        actions.AddToClassList("save-confirmation-actions");

        Button cancel = new(() => blocker.RemoveFromHierarchy()) { text = "CANCEL" };
        Button overwrite = new(() =>
        {
            blocker.RemoveFromHierarchy();
            confirm(slot);
        }) { text = "OVERWRITE" };
        cancel.AddToClassList("save-confirmation-button");
        overwrite.AddToClassList("save-confirmation-button");
        actions.Add(cancel);
        actions.Add(overwrite);
        dialog.Add(actions);
        blocker.Add(dialog);
        overlay.Add(blocker);
        blocker.BringToFront();
    }

    public static void ShowSaveSuccess(
        VisualElement overlay,
        int slot,
        Action dismiss)
    {
        if (overlay == null)
        {
            dismiss?.Invoke();
            return;
        }

        VisualElement existing = overlay.Q<VisualElement>("SaveSuccessConfirmation");
        existing?.RemoveFromHierarchy();

        VisualElement blocker = new() { name = "SaveSuccessConfirmation" };
        blocker.AddToClassList("save-confirmation-blocker");

        VisualElement dialog = new();
        dialog.AddToClassList("save-confirmation-dialog");

        Label title = new("GAME SAVED");
        title.AddToClassList("save-confirmation-title");
        dialog.Add(title);
        dialog.Add(new Label($"Successfully saved file to Slot {slot + 1}."));

        VisualElement actions = new();
        actions.AddToClassList("save-confirmation-actions");

        Button okay = new(() =>
        {
            blocker.RemoveFromHierarchy();
            dismiss?.Invoke();
        }) { text = "OK" };
        okay.AddToClassList("save-confirmation-button");
        actions.Add(okay);
        dialog.Add(actions);
        blocker.Add(dialog);
        overlay.Add(blocker);
        blocker.BringToFront();
        okay.Focus();
    }

    public static void Close(VisualElement overlay)
    {
        if (overlay != null) overlay.style.display = DisplayStyle.None;
    }

    private static Texture2D LoadThumbnail(int slot)
    {
        string path = SaveGameService.GetScreenshotPath(slot);
        if (!File.Exists(path)) return null;
        try
        {
            Texture2D texture = new(2, 2, TextureFormat.RGB24, false);
            return texture.LoadImage(File.ReadAllBytes(path)) ? texture : null;
        }
        catch { return null; }
    }
}
