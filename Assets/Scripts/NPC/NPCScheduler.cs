using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves NPCs between scheduled locations based on WorldStateManager time.
/// Each NPC has a DailySchedule asset listing where they should be each hour.
///
/// Requires a NavMeshAgent on the NPC GameObject.
/// NavMesh must be baked in each chapter scene.
/// </summary>
[RequireComponent(typeof(NPCBrain))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCScheduler : MonoBehaviour
{
    [Header("Schedule")]
    [Tooltip("One entry per time block. NPC moves to the listed destination at that hour.")]
    [SerializeField] private List<ScheduleEntry> _schedule = new List<ScheduleEntry>();

    [Header("Movement")]
    [SerializeField] private float _arrivalThreshold = 0.6f;

    private NavMeshAgent _agent;
    private NPCBrain     _brain;
    private int          _lastHour = -1;
    private ScheduleEntry _currentEntry;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _brain = GetComponent<NPCBrain>();
    }

    private void OnEnable()
        => EventBus.Subscribe<TimeOfDayChangedEvent>(OnTimeChanged);

    private void OnDisable()
        => EventBus.Unsubscribe<TimeOfDayChangedEvent>(OnTimeChanged);

    private void Update()
    {
        if (_brain.CurrentState != NPCBrain.NPCState.Scheduled) return;
        if (_agent == null || !_agent.isOnNavMesh) return;
        if (_agent.remainingDistance < _arrivalThreshold)
            _brain.TransitionTo(NPCBrain.NPCState.Idle);
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private void OnTimeChanged(TimeOfDayChangedEvent e)
    {
        int hour = Mathf.FloorToInt(e.Hour);
        if (hour == _lastHour) return;
        _lastHour = hour;

        var entry = GetEntryForHour(hour);
        if (entry == null || entry == _currentEntry) return;
        if (_brain.CurrentState == NPCBrain.NPCState.Talking) return;

        _currentEntry = entry;

        if (entry.Destination != null && _agent.isOnNavMesh)
        {
            _agent.SetDestination(entry.Destination.position);
            _brain.TransitionTo(NPCBrain.NPCState.Scheduled);
        }
    }

    private ScheduleEntry GetEntryForHour(int hour)
    {
        ScheduleEntry result = null;
        foreach (var entry in _schedule)
        {
            if (entry.StartHour <= hour)
                result = entry;
        }
        return result;
    }
}

// ─── Schedule data ────────────────────────────────────────────────────────────

[Serializable]
public class ScheduleEntry
{
    [Tooltip("Hour (0-23) this entry activates.")]
    public int       StartHour;

    [Tooltip("Transform marking the destination. Use an empty GameObject as a waypoint.")]
    public Transform Destination;

    [Tooltip("What the NPC is doing here — for debug and future animation state.")]
    public string    Activity;
}
