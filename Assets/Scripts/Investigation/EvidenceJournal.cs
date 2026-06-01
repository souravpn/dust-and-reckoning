using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the player's clue collection and the connection graph.
/// Clues are discovered via ClueObject interactions or Yarn <<unlock_clue>> commands.
/// Connections are made by the player in the Journal UI (drag thread between two cards).
///
/// When two clues are connected that form a valid deduction, a DeductionId is minted
/// and CluesConnectedEvent fires — Yarn commands and NPC dialogue options may react.
/// </summary>
public class EvidenceJournal : MonoBehaviour
{
    public static EvidenceJournal Instance { get; private set; }

    // Discovered clue IDs
    private readonly HashSet<string> _discoveredClues = new HashSet<string>();

    // Completed deduction IDs
    private readonly HashSet<string> _deductions = new HashSet<string>();

    // Valid connection pairs: key = sorted "A|B", value = deduction ID it produces
    // Populated at startup from JournalConnectionData ScriptableObject (post-MVP)
    // For MVP: hardcoded Act I chain
    private static readonly Dictionary<string, string> ValidConnections = new()
    {
        { "CalebMarshDeathNotice|DocAldridgeTestimony",          "Deduction_CertificateSignedEarly"     },
        { "Deduction_CertificateSignedEarly|HarrowMineReport",   "Deduction_TimingOfMurder"             },
        { "Deduction_TimingOfMurder|CoraMarshletter",            "Deduction_LedgerHiddenInMine"         },
        { "Deduction_LedgerHiddenInMine|HarrowMineLedger",       "Deduction_DrossLandFraud"             },
        { "PinkertonCipher|TheWidowLetter",                      "Deduction_EliasWasSentByDross"        }
    };

    // Active connections made by the player (not necessarily valid)
    private readonly HashSet<string> _playerConnections = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ClueDiscoveredEvent>(OnClueDiscovered);
        EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ClueDiscoveredEvent>(OnClueDiscovered);
        EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
    }

    // ── Public API ───────────────────────────────────────────────────────

    public void DiscoverClue(string clueId)
    {
        if (_discoveredClues.Add(clueId))
        {
            EventBus.Publish(new ClueDiscoveredEvent(clueId));
            Debug.Log($"[Journal] Clue discovered: {clueId}");
        }
    }

    /// <summary>
    /// Called by Journal UI when the player drags a thread between two clue cards.
    /// Returns the deduction ID if the connection is valid, null otherwise.
    /// </summary>
    public string TryConnect(string clueIdA, string clueIdB)
    {
        var key = MakeKey(clueIdA, clueIdB);
        _playerConnections.Add(key);

        if (ValidConnections.TryGetValue(key, out var deductionId))
        {
            if (_deductions.Add(deductionId))
            {
                // Deduction is new — discover it as a clue too
                DiscoverClue(deductionId);
                EventBus.Publish(new CluesConnectedEvent(clueIdA, clueIdB, deductionId));
                Debug.Log($"[Journal] Deduction made: {deductionId}");
                return deductionId;
            }
        }

        // Wrong connection — silent failure, no penalty
        return null;
    }

    public bool HasClue(string clueId)       => _discoveredClues.Contains(clueId);
    public bool HasDeduction(string id)      => _deductions.Contains(id);
    public int  TotalCluesDiscovered()       => _discoveredClues.Count;
    public IEnumerable<string> AllClues()    => _discoveredClues;
    public IEnumerable<string> AllDeductions() => _deductions;

    /// <summary>True if player has connected at least N valid deductions — used for
    /// Terrence bluff check and other threshold unlocks.</summary>
    public bool HasMinDeductions(int count) => _deductions.Count >= count;

    // ── Snapshot ─────────────────────────────────────────────────────────

    public JournalSnapshot Snapshot()
    {
        var snap = new JournalSnapshot();
        snap.DiscoveredClueIds.AddRange(_discoveredClues);
        snap.DeductionIds.AddRange(_deductions);
        snap.Connections.AddRange(_playerConnections);
        return snap;
    }

    public void ApplySnapshot(JournalSnapshot snap)
    {
        _discoveredClues.Clear();
        _deductions.Clear();
        _playerConnections.Clear();

        foreach (var id in snap.DiscoveredClueIds) _discoveredClues.Add(id);
        foreach (var id in snap.DeductionIds)      _deductions.Add(id);
        foreach (var id in snap.Connections)        _playerConnections.Add(id);
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private void OnClueDiscovered(ClueDiscoveredEvent e)
        => DiscoverClue(e.ClueId); // idempotent

    private void OnSceneLoaded(SceneLoadedEvent _)
    {
        if (SaveSystem.Instance?.CurrentSave?.Journal is { } snap)
            ApplySnapshot(snap);
    }

    /// <summary>Returns a canonical sorted key for a clue pair.</summary>
    private static string MakeKey(string a, string b)
        => string.Compare(a, b, System.StringComparison.Ordinal) < 0
            ? $"{a}|{b}"
            : $"{b}|{a}";
}
