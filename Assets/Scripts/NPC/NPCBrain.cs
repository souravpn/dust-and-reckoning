using UnityEngine;

/// <summary>
/// Base NPC state machine. All named NPCs inherit or use this component.
///
/// States: Idle → Scheduled → Talking → Alarmed
///
/// NPCScheduler advances Scheduled state based on WorldStateManager time.
/// DialogueTrigger transitions to/from Talking.
/// CombatManager sets Alarmed.
/// </summary>
[RequireComponent(typeof(NPCMemory))]
public class NPCBrain : MonoBehaviour
{
    public enum NPCState { Idle, Scheduled, Talking, Alarmed }

    public NPCState CurrentState { get; private set; } = NPCState.Idle;
    public NPCMemory Memory      { get; private set; }

    [Header("Detection")]
    [SerializeField] private float _detectionRange  = 15f;
    [SerializeField] private float _detectionAngle  = 120f; // degrees

    private void Awake() => Memory = GetComponent<NPCMemory>();

    public void TransitionTo(NPCState next)
    {
        if (next == CurrentState) return;
        var prev = CurrentState;
        CurrentState = next;
        OnStateChanged(prev, next);
    }

    protected virtual void OnStateChanged(NPCState from, NPCState to)
    {
        // Override in subclasses for custom behaviour per state transition
        Debug.Log($"[NPC:{Memory.NpcId}] {from} → {to}");
    }

    /// <summary>Returns true if the player is within detection cone.</summary>
    public bool CanDetectPlayer(Transform playerTransform)
    {
        var toPlayer = playerTransform.position - transform.position;
        if (toPlayer.magnitude > _detectionRange) return false;
        var angle = Vector3.Angle(transform.forward, toPlayer);
        return angle < _detectionAngle * 0.5f;
    }
}
