using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Serialises and deserialises the complete game save state to JSON.
/// Save file lives at Application.persistentDataPath/save.json.
///
/// Schema is versioned — increment SaveData.Version when fields are added
/// or removed, and handle migration in Load().
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SaveFileName = "save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public SaveData CurrentSave { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ───────────────────────────────────────────────────────

    public void NewGame()
    {
        CurrentSave = SaveData.CreateNew();
        Debug.Log("[SaveSystem] New game created.");
    }

    public void Save()
    {
        try
        {
            CollectFromSystems();
            var json = JsonUtility.ToJson(CurrentSave, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Saved to {SavePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed: {ex.Message}");
        }
    }

    public bool TryLoad()
    {
        if (!File.Exists(SavePath)) return false;

        try
        {
            var json = File.ReadAllText(SavePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);

            if (CurrentSave.Version < SaveData.CurrentVersion)
                MigrateSave(CurrentSave);

            ApplyToSystems();
            Debug.Log($"[SaveSystem] Loaded save v{CurrentSave.Version}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed: {ex.Message}");
            return false;
        }
    }

    public bool SaveExists() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("[SaveSystem] Save deleted.");
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private void CollectFromSystems()
    {
        if (ReputationManager.Instance != null)
            CurrentSave.Reputation = ReputationManager.Instance.Snapshot();

        if (EvidenceJournal.Instance != null)
            CurrentSave.Journal = EvidenceJournal.Instance.Snapshot();

        if (WorldStateManager.Instance != null)
            CurrentSave.WorldState = WorldStateManager.Instance.Snapshot();
    }

    private void ApplyToSystems()
    {
        // Systems pull from CurrentSave in their own Init() calls,
        // triggered by SceneLoadedEvent. No direct push needed here.
    }

    private void MigrateSave(SaveData save)
    {
        // Future migration logic goes here, keyed on save.Version
        save.Version = SaveData.CurrentVersion;
        Debug.Log("[SaveSystem] Save migrated to current version.");
    }
}

// ─── Save data schema ─────────────────────────────────────────────────────────

[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int Version = CurrentVersion;
    public string CurrentChapter;
    public float PlaytimeSeconds;

    public ReputationSnapshot Reputation;
    public JournalSnapshot Journal;
    public WorldStateSnapshot WorldState;

    public string HorseName;
    public int HorseLoyalty;
    public int HorseCondition;

    public float PlayerMoney;

    public static SaveData CreateNew() => new SaveData
    {
        Version        = CurrentVersion,
        CurrentChapter = "A1_C1_BlackwoodStation",
        PlaytimeSeconds = 0f,
        Reputation     = ReputationSnapshot.Default(),
        Journal        = new JournalSnapshot(),
        WorldState     = new WorldStateSnapshot(),
        HorseName      = "",
        HorseLoyalty   = 50,
        HorseCondition = 100,
        PlayerMoney    = 18f
    };
}

[Serializable]
public class ReputationSnapshot
{
    public int Law;
    public int Outlaws;
    public int Townsfolk;
    public int Shoshone;

    public static ReputationSnapshot Default() => new ReputationSnapshot
    { Law = 0, Outlaws = 0, Townsfolk = 0, Shoshone = 0 };
}

[Serializable]
public class JournalSnapshot
{
    public List<string> DiscoveredClueIds = new List<string>();
    public List<string> DeductionIds      = new List<string>();
    // Connections stored as "clueA|clueB" pairs
    public List<string> Connections       = new List<string>();
}

[Serializable]
public class WorldStateSnapshot
{
    public float     TimeOfDay    = 10f; // Hour (0–24)
    public WeatherType Weather    = WeatherType.Clear;
    public List<string> SetFlags  = new List<string>();
}
