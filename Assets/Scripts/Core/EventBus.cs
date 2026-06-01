using System;
using System.Collections.Generic;

/// <summary>
/// Global publish/subscribe event bus. All cross-system communication goes
/// through here — no manager holds a direct reference to another.
///
/// Usage:
///   Publishing:   EventBus.Publish(new ReputationChangedEvent(Faction.Law, -10));
///   Subscribing:  EventBus.Subscribe<ReputationChangedEvent>(OnRepChanged);
///   Unsubscribing: EventBus.Unsubscribe<ReputationChangedEvent>(OnRepChanged);
///
/// Subscribe in OnEnable / Awake. Always unsubscribe in OnDisable / OnDestroy
/// to prevent memory leaks from destroyed listeners.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers
        = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();
        _handlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var list))
            list.Remove(handler);
    }

    public static void Publish<T>(T gameEvent) where T : IGameEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list)) return;

        // Iterate a copy — handlers may unsubscribe during dispatch
        var snapshot = new List<Delegate>(list);
        foreach (var handler in snapshot)
            (handler as Action<T>)?.Invoke(gameEvent);
    }

    /// <summary>
    /// Clears all subscriptions. Call on scene teardown if needed,
    /// but prefer unsubscribing individually in OnDisable/OnDestroy.
    /// </summary>
    public static void Clear() => _handlers.Clear();
}

// ─── Marker interface ────────────────────────────────────────────────────────

public interface IGameEvent { }

// ─── Core game events ────────────────────────────────────────────────────────

public readonly struct GameStateChangedEvent : IGameEvent
{
    public readonly GameState Previous;
    public readonly GameState Next;
    public GameStateChangedEvent(GameState previous, GameState next)
    { Previous = previous; Next = next; }
}

public readonly struct SceneLoadRequestedEvent : IGameEvent
{
    public readonly string SceneName;
    public readonly bool Additive;
    public SceneLoadRequestedEvent(string sceneName, bool additive = false)
    { SceneName = sceneName; Additive = additive; }
}

public readonly struct SceneLoadedEvent : IGameEvent
{
    public readonly string SceneName;
    public SceneLoadedEvent(string sceneName) { SceneName = sceneName; }
}

// ─── Player events ───────────────────────────────────────────────────────────

public readonly struct PlayerHealthChangedEvent : IGameEvent
{
    public readonly float Previous;
    public readonly float Current;
    public readonly float Max;
    public PlayerHealthChangedEvent(float previous, float current, float max)
    { Previous = previous; Current = current; Max = max; }
}

public readonly struct PlayerDiedEvent : IGameEvent { }

public readonly struct PlayerEnteredCombatEvent : IGameEvent { }

public readonly struct PlayerExitedCombatEvent : IGameEvent { }

// ─── Reputation events ───────────────────────────────────────────────────────

public readonly struct ReputationChangedEvent : IGameEvent
{
    public readonly Faction Faction;
    public readonly int Delta;
    public readonly int NewValue;
    public ReputationChangedEvent(Faction faction, int delta, int newValue)
    { Faction = faction; Delta = delta; NewValue = newValue; }
}

public readonly struct ReputationThresholdCrossedEvent : IGameEvent
{
    public readonly Faction Faction;
    public readonly ReputationTier NewTier;
    public ReputationThresholdCrossedEvent(Faction faction, ReputationTier newTier)
    { Faction = faction; NewTier = newTier; }
}

// ─── Dialogue events ─────────────────────────────────────────────────────────

public readonly struct DialogueStartedEvent : IGameEvent
{
    public readonly string NpcId;
    public DialogueStartedEvent(string npcId) { NpcId = npcId; }
}

public readonly struct DialogueEndedEvent : IGameEvent
{
    public readonly string NpcId;
    public DialogueEndedEvent(string npcId) { NpcId = npcId; }
}

public readonly struct DialogueToneSetEvent : IGameEvent
{
    public readonly string NpcId;
    public readonly DialogueTone Tone;
    public DialogueToneSetEvent(string npcId, DialogueTone tone)
    { NpcId = npcId; Tone = tone; }
}

// ─── Investigation events ────────────────────────────────────────────────────

public readonly struct ClueDiscoveredEvent : IGameEvent
{
    public readonly string ClueId;
    public ClueDiscoveredEvent(string clueId) { ClueId = clueId; }
}

public readonly struct CluesConnectedEvent : IGameEvent
{
    public readonly string ClueIdA;
    public readonly string ClueIdB;
    public readonly string DeductionId;
    public CluesConnectedEvent(string clueIdA, string clueIdB, string deductionId)
    { ClueIdA = clueIdA; ClueIdB = clueIdB; DeductionId = deductionId; }
}

// ─── World events ────────────────────────────────────────────────────────────

public readonly struct TimeOfDayChangedEvent : IGameEvent
{
    public readonly float Hour; // 0–24
    public TimeOfDayChangedEvent(float hour) { Hour = hour; }
}

public readonly struct WeatherChangedEvent : IGameEvent
{
    public readonly WeatherType Weather;
    public WeatherChangedEvent(WeatherType weather) { Weather = weather; }
}

public readonly struct WorldFlagSetEvent : IGameEvent
{
    public readonly string FlagKey;
    public readonly bool Value;
    public WorldFlagSetEvent(string flagKey, bool value)
    { FlagKey = flagKey; Value = value; }
}

// ─── Audio events ────────────────────────────────────────────────────────────

public readonly struct MusicIntensityChangedEvent : IGameEvent
{
    public readonly float Intensity; // 0–1
    public MusicIntensityChangedEvent(float intensity) { Intensity = intensity; }
}

// ─── Horse events ────────────────────────────────────────────────────────────

public readonly struct HorseLoyaltyChangedEvent : IGameEvent
{
    public readonly int Delta;
    public readonly int NewValue;
    public HorseLoyaltyChangedEvent(int delta, int newValue)
    { Delta = delta; NewValue = newValue; }
}
