using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns all four faction reputation values (-100 to +100).
/// Only source of truth — nothing else stores reputation numbers.
///
/// Changes are applied via ChangeReputation() or via Yarn command
/// <<reputation_change Faction delta>>.
///
/// Changes are NEVER announced via UI popup. The HUD reads tier color
/// (ReputationTier) via GetTier(). Raw values are accessible only
/// through the Journal > People screen.
/// </summary>
public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }

    private readonly Dictionary<Faction, int> _values = new()
    {
        { Faction.Law,       0 },
        { Faction.Outlaws,   0 },
        { Faction.Townsfolk, 0 },
        { Faction.Shoshone,  0 }
    };

    // June starts the player at +10 Shoshone
    private const int ShoshoneStartBonus = 10;

    private const int Min = -100;
    private const int Max =  100;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _values[Faction.Shoshone] = ShoshoneStartBonus;
    }

    private void OnEnable()  => EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
    private void OnDisable() => EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);

    private void OnSceneLoaded(SceneLoadedEvent _)
    {
        // Re-apply saved values if a save is loaded
        if (SaveSystem.Instance?.CurrentSave?.Reputation is { } snap)
            ApplySnapshot(snap);
    }

    // ── Public API ───────────────────────────────────────────────────────

    public void ChangeReputation(Faction faction, int delta)
    {
        if (delta == 0) return;

        var prev  = _values[faction];
        var next  = Mathf.Clamp(prev + delta, Min, Max);
        _values[faction] = next;

        var prevTier = ToTier(prev);
        var nextTier = ToTier(next);

        EventBus.Publish(new ReputationChangedEvent(faction, delta, next));

        if (prevTier != nextTier)
            EventBus.Publish(new ReputationThresholdCrossedEvent(faction, nextTier));

        Debug.Log($"[Reputation] {faction}: {prev} → {next} ({(delta >= 0 ? "+" : "")}{delta})");
    }

    public int  GetValue(Faction faction) => _values[faction];
    public ReputationTier GetTier(Faction faction) => ToTier(_values[faction]);

    /// <summary>
    /// Returns true if the player meets or exceeds a threshold for a faction.
    /// Use for unlock checks (e.g. door access, dialogue options).
    /// </summary>
    public bool Meets(Faction faction, int threshold)
        => _values[faction] >= threshold;

    // ── Snapshot (for save system) ───────────────────────────────────────

    public ReputationSnapshot Snapshot() => new ReputationSnapshot
    {
        Law       = _values[Faction.Law],
        Outlaws   = _values[Faction.Outlaws],
        Townsfolk = _values[Faction.Townsfolk],
        Shoshone  = _values[Faction.Shoshone]
    };

    public void ApplySnapshot(ReputationSnapshot snap)
    {
        _values[Faction.Law]       = Mathf.Clamp(snap.Law,       Min, Max);
        _values[Faction.Outlaws]   = Mathf.Clamp(snap.Outlaws,   Min, Max);
        _values[Faction.Townsfolk] = Mathf.Clamp(snap.Townsfolk, Min, Max);
        _values[Faction.Shoshone]  = Mathf.Clamp(snap.Shoshone,  Min, Max);
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private static ReputationTier ToTier(int value) => value switch
    {
        >= 60  => ReputationTier.Allied,
        >= 20  => ReputationTier.Warm,
        > -20  => ReputationTier.Neutral,
        > -60  => ReputationTier.Cold,
        _      => ReputationTier.Hostile
    };
}
