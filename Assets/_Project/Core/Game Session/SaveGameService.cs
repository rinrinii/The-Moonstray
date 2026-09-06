using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameService
{
    public const int SlotCount = 3;
    public readonly struct SlotInfo
    {
        public readonly bool Exists;
        public readonly string Location;
        public readonly string MainQuest;
        public readonly string SavedAt;

        public SlotInfo(bool exists, string location, string mainQuest, string savedAt)
        {
            Exists = exists;
            Location = location;
            MainQuest = mainQuest;
            SavedAt = savedAt;
        }
    }

    [Serializable]
    private sealed class SaveFile
    {
        public int fileVersion = CurrentFileVersion;
        public string savedAtUtc;
        public string location;
        public string mainQuestSummary;
        public GameSessionManager.Snapshot session;
    }

    private const int CurrentFileVersion = 1;
    private const string SaveDirectoryName = "Saves";
    private const string SaveFileName = "save-slot-{0}.json";
    private const string BackupSuffix = ".backup";
    private const string TemporarySuffix = ".tmp";

    private static int currentSlot;
    public static string SavePath => GetSavePath(currentSlot);
    public static string GetSavePath(int slot) => Path.Combine(
        Application.persistentDataPath, SaveDirectoryName,
        string.Format(SaveFileName, Mathf.Clamp(slot, 0, SlotCount - 1)));
    public static string GetScreenshotPath(int slot) =>
        GetSavePath(slot) + ".png";

    private static string BackupPath => SavePath + BackupSuffix;
    private static string TemporaryPath => SavePath + TemporarySuffix;

    public static bool HasValidSave()
    {
        return TryReadSave(SavePath, out _, out _) ||
            TryReadSave(BackupPath, out _, out _);
    }

    public static bool HasValidSave(int slot)
    {
        currentSlot = Mathf.Clamp(slot, 0, SlotCount - 1);
        return HasValidSave();
    }

    public static bool HasAnyValidSave()
    {
        for (int slot = 0; slot < SlotCount; slot++)
            if (HasValidSave(slot)) return true;
        return false;
    }

    public static bool TryGetSlotInfo(int slot, out SlotInfo info)
    {
        currentSlot = Mathf.Clamp(slot, 0, SlotCount - 1);
        if (!TryReadSaveFile(SavePath, out SaveFile file) &&
            !TryReadSaveFile(BackupPath, out file))
        {
            info = new SlotInfo(false, string.Empty, string.Empty, string.Empty);
            return false;
        }

        string quest = file.mainQuestSummary;
        if (string.IsNullOrWhiteSpace(quest))
            quest = GetMainQuestSummary(file.session.quests);
        if (string.IsNullOrWhiteSpace(quest)) quest = "No active main quest";
        string savedAt = DateTime.TryParse(file.savedAtUtc, out DateTime time)
            ? time.ToLocalTime().ToString("g") : "Unknown time";
        info = new SlotInfo(true, file.location, quest, savedAt);
        return true;
    }

    public static bool TrySave(out string error)
    {
        error = null;

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name == "MainMenu" || scene.name == "LoadingScene")
        {
            error = "Saving is only available during gameplay.";
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(scene.name))
        {
            error = $"Scene '{scene.name}' is not included in Build Settings.";
            return false;
        }

        GameSessionManager.Snapshot snapshot = GameSessionManager.CaptureState();
        if (snapshot?.world == null || !snapshot.world.hasPlayerState)
        {
            error = "The player is not ready to save.";
            return false;
        }

        SaveFile saveFile = new()
        {
            savedAtUtc = DateTime.UtcNow.ToString("O"),
            location = snapshot.world.sceneName,
            mainQuestSummary = GetMainQuestSummary(snapshot.quests),
            session = snapshot
        };

        try
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                error = "The save directory is unavailable.";
                return false;
            }

            Directory.CreateDirectory(directory);
            WriteTemporaryFile(JsonUtility.ToJson(saveFile, true));
            CommitTemporaryFile();
            TryCaptureScreenshot(currentSlot);
            return true;
        }
        catch (Exception exception)
        {
            TryDeleteTemporaryFile();
            error = $"Could not write the save file: {exception.Message}";
            Debug.LogException(exception);
            return false;
        }
    }

    public static bool TrySave(int slot, out string error)
    {
        currentSlot = Mathf.Clamp(slot, 0, SlotCount - 1);
        return TrySave(out error);
    }

    public static bool TryLoad(out GameSessionManager.Snapshot snapshot, out string error)
    {
        if (TryReadSave(SavePath, out snapshot, out error))
            return true;

        string primaryError = error;
        if (TryReadSave(BackupPath, out snapshot, out string backupError))
        {
            Debug.LogWarning($"Primary save unavailable ({primaryError}). Loaded the backup save.");
            error = null;
            return true;
        }

        snapshot = null;
        error = File.Exists(SavePath) || File.Exists(BackupPath)
            ? $"No valid save could be loaded. Primary: {primaryError} Backup: {backupError}"
            : "No save file exists yet.";
        return false;
    }

    public static bool TryLoad(int slot, out GameSessionManager.Snapshot snapshot, out string error)
    {
        currentSlot = Mathf.Clamp(slot, 0, SlotCount - 1);
        return TryLoad(out snapshot, out error);
    }

    private static bool TryReadSaveFile(string path, out SaveFile file)
    {
        file = null;
        if (!TryReadSave(path, out _, out _)) return false;
        try
        {
            file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(path, Encoding.UTF8));
            return file != null;
        }
        catch { return false; }
    }

    private static string GetMainQuestSummary(QuestManager.Snapshot quests)
    {
        if (quests == null)
            return string.Empty;

        QuestManager.QuestRecord latest = quests.currentMainQuest;
        if (quests.objectiveJournalQuests != null)
        {
            foreach (QuestManager.QuestRecord candidate in quests.objectiveJournalQuests)
            {
                if (candidate == null || candidate.completed)
                    continue;

                if (latest == null || candidate.lastUpdatedOrder > latest.lastUpdatedOrder)
                    latest = candidate;
            }
        }

        if (latest == null || string.IsNullOrWhiteSpace(latest.title))
            return string.Empty;

        string objectiveText = string.Empty;
        if (latest.objectives != null)
        {
            QuestManager.ObjectiveRecord current = latest.objectives.Find(
                objective => objective != null && objective.objectiveID == latest.currentObjectiveID);
            current ??= latest.objectives.Find(
                objective => objective != null && !objective.completed);
            objectiveText = current?.text;
        }

        return string.IsNullOrWhiteSpace(objectiveText)
            ? latest.title
            : $"{latest.title}: {objectiveText}";
    }

    private static bool TryReadSave(
        string path,
        out GameSessionManager.Snapshot snapshot,
        out string error)
    {
        snapshot = null;
        error = null;

        if (!File.Exists(path))
        {
            error = "File not found.";
            return false;
        }

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "File is empty.";
                return false;
            }

            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);
            if (saveFile == null || saveFile.session == null)
            {
                error = "Save data is incomplete.";
                return false;
            }

            if (saveFile.fileVersion <= 0 || saveFile.fileVersion > CurrentFileVersion)
            {
                error = $"Unsupported save-file version {saveFile.fileVersion}.";
                return false;
            }

            if (saveFile.session.schemaVersion <= 0 ||
                saveFile.session.schemaVersion > GameSessionManager.CurrentSchemaVersion)
            {
                error = $"Unsupported session version {saveFile.session.schemaVersion}.";
                return false;
            }

            if (saveFile.session.world == null ||
                string.IsNullOrWhiteSpace(saveFile.session.world.sceneName))
            {
                error = "Save location is missing.";
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(saveFile.session.world.sceneName))
            {
                error = $"Saved scene '{saveFile.session.world.sceneName}' is unavailable.";
                return false;
            }

            snapshot = saveFile.session;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static void WriteTemporaryFile(string json)
    {
        using FileStream stream = new(
            TemporaryPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        using StreamWriter writer = new(stream, new UTF8Encoding(false));
        writer.Write(json);
        writer.Flush();
        stream.Flush(true);
    }

    private static void CommitTemporaryFile()
    {
        if (!File.Exists(SavePath))
        {
            File.Move(TemporaryPath, SavePath);
            return;
        }

        try
        {
            File.Replace(TemporaryPath, SavePath, BackupPath);
        }
        catch (PlatformNotSupportedException)
        {
            CommitWithPortableFallback();
        }
        catch (IOException)
        {
            CommitWithPortableFallback();
        }
    }

    private static void CommitWithPortableFallback()
    {
        File.Copy(SavePath, BackupPath, true);
        File.Delete(SavePath);
        File.Move(TemporaryPath, SavePath);
    }

    private static void TryDeleteTemporaryFile()
    {
        try
        {
            if (File.Exists(TemporaryPath))
                File.Delete(TemporaryPath);
        }
        catch (Exception)
        {
            // Preserve the original save error; a stale temp file is harmless.
        }
    }

    private static void TryCaptureScreenshot(int slot)
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        RenderTexture target = RenderTexture.GetTemporary(480, 270, 24);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            Texture2D image = new(480, 270, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 480, 270), 0, 0);
            image.Apply();
            File.WriteAllBytes(GetScreenshotPath(slot), image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save thumbnail could not be captured: {exception.Message}");
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
        }
    }
}
