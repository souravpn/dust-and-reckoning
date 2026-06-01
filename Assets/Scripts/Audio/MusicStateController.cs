using UnityEngine;

/// <summary>
/// The ONLY writer of the FMOD GameIntensity parameter.
/// Listens to game events and translates them into a 0–1 intensity float
/// that drives the adaptive score.
///
/// Intensity targets (from GDD §9):
///   0.00–0.25  Ambient exploration
///   0.25–0.45  Unease (night, approaching danger)
///   0.45–0.65  Tension (stealth, NPC suspicious)
///   0.65–0.85  Confrontation (combat)
///   0.85–1.00  Peak (boss, chapter climax)
/// </summary>
public class MusicStateController : MonoBehaviour
{
    [Header("Transition Speed")]
    [SerializeField] private float _lerpSpeed = 0.5f; // units/second

    private float _targetIntensity;
    private float _currentIntensity;

    // Intensity contributions — highest active value wins
    private bool _isCombatActive;
    private bool _isStealthActive;
    private bool _isNight;
    private bool _isBossEncounter;
    private bool _isNpcSuspicious;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerEnteredCombatEvent>(OnCombatEntered);
        EventBus.Subscribe<PlayerExitedCombatEvent>(OnCombatExited);
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Subscribe<TimeOfDayChangedEvent>(OnTimeChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerEnteredCombatEvent>(OnCombatEntered);
        EventBus.Unsubscribe<PlayerExitedCombatEvent>(OnCombatExited);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<TimeOfDayChangedEvent>(OnTimeChanged);
    }

    private void Update()
    {
        RecalculateTarget();

        if (Mathf.Approximately(_currentIntensity, _targetIntensity)) return;

        _currentIntensity = Mathf.MoveTowards(
            _currentIntensity, _targetIntensity, _lerpSpeed * Time.deltaTime);

        EventBus.Publish(new MusicIntensityChangedEvent(_currentIntensity));
    }

    // ── Public API (called by scripted story beats) ───────────────────

    public void SetBossEncounter(bool active)
    {
        _isBossEncounter = active;
        RecalculateTarget();
    }

    public void SetNpcSuspicious(bool suspicious)
    {
        _isNpcSuspicious = suspicious;
        RecalculateTarget();
    }

    public void SetStealthActive(bool active)
    {
        _isStealthActive = active;
        RecalculateTarget();
    }

    // ── Internal ─────────────────────────────────────────────────────

    private void RecalculateTarget()
    {
        if (_isBossEncounter)       { _targetIntensity = 0.90f; return; }
        if (_isCombatActive)        { _targetIntensity = 0.75f; return; }
        if (_isStealthActive || _isNpcSuspicious)
                                    { _targetIntensity = 0.55f; return; }
        if (_isNight)               { _targetIntensity = 0.30f; return; }
                                      _targetIntensity = 0.10f;
    }

    private void OnCombatEntered(PlayerEnteredCombatEvent _)
    { _isCombatActive = true; }

    private void OnCombatExited(PlayerExitedCombatEvent _)
    { _isCombatActive = false; }

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        if (e.Next == GameState.Dialogue || e.Next == GameState.Cutscene)
            _targetIntensity = Mathf.Min(_targetIntensity, 0.20f);
    }

    private void OnTimeChanged(TimeOfDayChangedEvent e)
        => _isNight = e.Hour is >= 20 or < 5;
}
