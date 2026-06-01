using UnityEngine;

/// <summary>
/// Sits on any NPC or interactable that can start a Yarn Spinner conversation.
/// Registers the Yarn commands that bridge dialogue to game systems.
///
/// Yarn commands registered here:
///   <<reputation_change Faction delta>>
///   <<unlock_clue clueId>>
///   <<connect_clues clueIdA clueIdB>>
///   <<set_flag key value>>
///   <<music_state intensity>>
///   <<npc_flag key value>>
///   <<mark_topic topicId>>
/// </summary>
[RequireComponent(typeof(NPCBrain))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn")]
    [Tooltip("The Yarn node to start conversation with. Must match a node in the .yarn file.")]
    [SerializeField] private string _defaultStartNode = "Start";

    [Tooltip("Optional override node for second and subsequent conversations.")]
    [SerializeField] private string _repeatStartNode  = "";

    private NPCBrain _brain;

    private void Awake() => _brain = GetComponent<NPCBrain>();

    // Called by PlayerController when player taps this NPC
    public void BeginDialogue()
    {
        if (_brain.CurrentState == NPCBrain.NPCState.Alarmed) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        var startNode = _brain.Memory.HasBeenMet() && !string.IsNullOrEmpty(_repeatStartNode)
            ? _repeatStartNode
            : _defaultStartNode;

        GameManager.Instance.BeginDialogue();
        _brain.TransitionTo(NPCBrain.NPCState.Talking);

        // YarnSpinner call — uncomment when package is installed:
        // var runner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        // runner.StartDialogue(startNode);

        Debug.Log($"[Dialogue] Started: {_brain.Memory.NpcId} / {startNode}");
    }

    public void EndDialogue()
    {
        _brain.TransitionTo(NPCBrain.NPCState.Idle);
        GameManager.Instance?.EndDialogue();
    }

    // ── Yarn command handlers ─────────────────────────────────────────
    // Register these with YarnSpinner's DialogueRunner in Bootstrap.
    // [YarnCommand("reputation_change")] when FMOD package is installed.

    public static void YarnCmd_ReputationChange(string factionStr, string deltaStr)
    {
        if (!System.Enum.TryParse<Faction>(factionStr, out var faction)) return;
        if (!int.TryParse(deltaStr, out var delta)) return;
        ReputationManager.Instance?.ChangeReputation(faction, delta);
    }

    public static void YarnCmd_UnlockClue(string clueId)
        => EvidenceJournal.Instance?.DiscoverClue(clueId);

    public static void YarnCmd_ConnectClues(string clueIdA, string clueIdB)
        => EvidenceJournal.Instance?.TryConnect(clueIdA, clueIdB);

    public static void YarnCmd_SetFlag(string key, string valueStr)
    {
        if (bool.TryParse(valueStr, out var value))
            WorldStateManager.Instance?.SetFlag(key, value);
    }

    public static void YarnCmd_MusicState(string intensityStr)
    {
        if (float.TryParse(intensityStr, out var intensity))
            EventBus.Publish(new MusicIntensityChangedEvent(Mathf.Clamp01(intensity)));
    }
}
