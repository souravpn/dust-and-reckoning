using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores per-NPC conversation history: tone history, flags set during
/// dialogue, and topics already discussed.
///
/// One NPCMemory component lives on each NPC prefab. Its state is
/// serialised into SaveData via WorldStateManager flags (prefix: "NPC_{id}_").
///
/// DialogueTrigger reads from this to pick the correct Yarn start node
/// and to filter available reply options.
/// </summary>
public class NPCMemory : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique stable ID for this NPC — used as save key prefix.")]
    [SerializeField] public string NpcId;

    // Dominant tone the player has used with this NPC
    public DialogueTone DominantTone { get; private set; } = DialogueTone.Neutral;

    // Conversation outcome flags (e.g. "askedAboutMarsh", "wasChallenged")
    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

    // Topics covered in previous conversations
    private readonly HashSet<string> _coveredTopics = new HashSet<string>();

    // Tone history (most recent N interactions)
    private const int ToneHistoryMax = 5;
    private readonly Queue<DialogueTone> _toneHistory = new Queue<DialogueTone>();

    // ── Public API ───────────────────────────────────────────────────────

    public void RecordTone(DialogueTone tone)
    {
        _toneHistory.Enqueue(tone);
        if (_toneHistory.Count > ToneHistoryMax)
            _toneHistory.Dequeue();

        DominantTone = ComputeDominant();
        EventBus.Publish(new DialogueToneSetEvent(NpcId, DominantTone));
    }

    public void SetFlag(string key, bool value)
    {
        _flags[key] = value;

        // Mirror into WorldStateManager with NPC prefix for save/load
        WorldStateManager.Instance?.SetFlag($"NPC_{NpcId}_{key}", value);
    }

    public bool GetFlag(string key)
        => _flags.TryGetValue(key, out var v) && v;

    public void MarkTopicCovered(string topic)
        => _coveredTopics.Add(topic);

    public bool HasCoveredTopic(string topic)
        => _coveredTopics.Contains(topic);

    public bool HasBeenMet() => _toneHistory.Count > 0;

    // ── Internal ─────────────────────────────────────────────────────────

    private DialogueTone ComputeDominant()
    {
        var counts = new Dictionary<DialogueTone, int>();
        foreach (var t in _toneHistory)
        {
            if (!counts.ContainsKey(t)) counts[t] = 0;
            counts[t]++;
        }

        DialogueTone dominant = DialogueTone.Neutral;
        int max = 0;
        foreach (var kv in counts)
        {
            if (kv.Value > max) { max = kv.Value; dominant = kv.Key; }
        }
        return dominant;
    }
}
