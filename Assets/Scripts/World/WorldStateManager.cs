using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the world's mutable boolean flags and the in-game clock.
/// Flags drive conditional NPC behaviour, unlock checks, and Yarn dialogue branches.
///
/// Set flags from Yarn:  <<set_flag FlagName true>>
/// Check flags from C#:  WorldStateManager.Instance.GetFlag("FlagName")
/// </summary>
public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    // ── Clock ──────────────────────────────────────────────────────────
    // 24-minute real-time day: 1 game-hour = 2 real-seconds
    [Header("Day/Night")]
    [SerializeField] private float _startHour         = 10f;  // 10:00 AM on arrival
    [SerializeField] private float _realSecondsPerDay = 24f * 2f; // 48s = full day

    public float  TimeOfDay   { get; private set; }  // 0–24
    public int    Hour        => Mathf.FloorToInt(TimeOfDay);
    public int    Minute      => Mathf.FloorToInt((TimeOfDay % 1f) * 60f);
    public bool   IsNight     => TimeOfDay is >= 20f or < 5f;

    private float _secondsPerGameHour;

    // ── Flags ──────────────────────────────────────────────────────────
    private readonly HashSet<string> _trueFlags = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _secondsPerGameHour = _realSecondsPerDay / 24f;
        TimeOfDay = _startHour;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<WorldFlagSetEvent>(OnFlagSet);
        EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<WorldFlagSetEvent>(OnFlagSet);
        EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        var previousHour = Hour;
        TimeOfDay = (TimeOfDay + Time.deltaTime / _secondsPerGameHour) % 24f;

        if (Hour != previousHour)
            EventBus.Publish(new TimeOfDayChangedEvent(TimeOfDay));
    }

    // ── Clock API ──────────────────────────────────────────────────────

    /// <summary>Advance the clock by a number of game-hours (fast travel, sleep).</summary>
    public void AdvanceTime(float hours)
    {
        TimeOfDay = (TimeOfDay + hours) % 24f;
        EventBus.Publish(new TimeOfDayChangedEvent(TimeOfDay));
    }

    public string FormattedTime()
    {
        var h = Hour % 12 == 0 ? 12 : Hour % 12;
        var suffix = Hour < 12 ? "AM" : "PM";
        return $"{h}:{Minute:D2} {suffix}";
    }

    // ── Flags API ──────────────────────────────────────────────────────

    public void  SetFlag(string key, bool value)
    {
        if (value) _trueFlags.Add(key);
        else       _trueFlags.Remove(key);

        EventBus.Publish(new WorldFlagSetEvent(key, value));
    }

    public bool GetFlag(string key) => _trueFlags.Contains(key);

    // ── Snapshot ───────────────────────────────────────────────────────

    public WorldStateSnapshot Snapshot() => new WorldStateSnapshot
    {
        TimeOfDay = TimeOfDay,
        SetFlags  = new List<string>(_trueFlags)
    };

    public void ApplySnapshot(WorldStateSnapshot snap)
    {
        TimeOfDay = snap.TimeOfDay;
        _trueFlags.Clear();
        foreach (var f in snap.SetFlags) _trueFlags.Add(f);
    }

    // ── Internal ───────────────────────────────────────────────────────

    private void OnFlagSet(WorldFlagSetEvent e) { /* Already handled above */ }

    private void OnSceneLoaded(SceneLoadedEvent _)
    {
        if (SaveSystem.Instance?.CurrentSave?.WorldState is { } snap)
            ApplySnapshot(snap);
    }
}
