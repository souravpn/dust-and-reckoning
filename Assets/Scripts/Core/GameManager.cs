using UnityEngine;

/// <summary>
/// Central game state machine. Owns the current GameState and transitions
/// between states. All other systems react to GameStateChangedEvent rather
/// than querying GameManager directly.
///
/// Initialized by Bootstrap scene. Persists for the lifetime of the session.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Initializing;

    [Header("Startup")]
    [SerializeField] private string _mainMenuScene = "MainMenu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TransitionTo(GameState.MainMenu);
        EventBus.Publish(new SceneLoadRequestedEvent(_mainMenuScene));
    }

    public void TransitionTo(GameState next)
    {
        if (next == CurrentState) return;

        var previous = CurrentState;
        CurrentState = next;

        EventBus.Publish(new GameStateChangedEvent(previous, next));

        Debug.Log($"[GameManager] {previous} → {next}");
    }

    // ── Convenience state checks ──────────────────────────────────────────

    public bool IsPlaying     => CurrentState == GameState.Playing;
    public bool IsPaused      => CurrentState == GameState.Paused;
    public bool IsInDialogue  => CurrentState == GameState.Dialogue;
    public bool IsInCutscene  => CurrentState == GameState.Cutscene;

    // ── Input-driven transitions (called by UI / input handler) ───────────

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            TransitionTo(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
            TransitionTo(GameState.Playing);
    }

    public void OpenInventory()
    {
        if (CurrentState == GameState.Playing)
            TransitionTo(GameState.Inventory);
    }

    public void OpenJournal()
    {
        if (CurrentState == GameState.Playing)
            TransitionTo(GameState.Journal);
    }

    public void OpenMap()
    {
        if (CurrentState == GameState.Playing)
            TransitionTo(GameState.Map);
    }

    public void CloseOverlay()
    {
        // Return to Playing from any overlay state
        if (CurrentState == GameState.Inventory ||
            CurrentState == GameState.Journal   ||
            CurrentState == GameState.Map       ||
            CurrentState == GameState.Paused)
        {
            TransitionTo(GameState.Playing);
        }
    }

    public void BeginDialogue()
    {
        if (CurrentState == GameState.Playing)
            TransitionTo(GameState.Dialogue);
    }

    public void EndDialogue()
    {
        if (CurrentState == GameState.Dialogue)
            TransitionTo(GameState.Playing);
    }

    public void BeginCutscene()  => TransitionTo(GameState.Cutscene);
    public void EndCutscene()    => TransitionTo(GameState.Playing);
}
