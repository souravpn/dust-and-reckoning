using UnityEngine;

/// <summary>
/// Wraps FMOD Studio integration. All audio calls in the codebase go
/// through this class — nothing calls FMOD directly.
///
/// Requires the FMOD Unity integration package (ThirdParty/FMOD).
/// In pre-production (no FMOD installed), all methods fail silently
/// so other systems can be developed without audio dependency.
///
/// Music is driven by a single FMOD parameter "GameIntensity" (0–1).
/// MusicStateController.cs is the only writer of this parameter.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("FMOD Banks")]
    [SerializeField] private string _masterBankPath   = "Master";
    [SerializeField] private string _musicBankPath    = "Music";
    [SerializeField] private string _sfxBankPath      = "SFX";
    [SerializeField] private string _dialogueBankPath = "Dialogue";

    [Header("Music Parameter")]
    [SerializeField] private string _intensityParam = "GameIntensity";

    private bool _fmodAvailable;
    private float _currentIntensity;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _fmodAvailable = CheckFMOD();
        if (_fmodAvailable) LoadBanks();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<MusicIntensityChangedEvent>(OnIntensityChanged);
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MusicIntensityChangedEvent>(OnIntensityChanged);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    // ── Music ─────────────────────────────────────────────────────────

    public void SetMusicIntensity(float intensity)
    {
        _currentIntensity = Mathf.Clamp01(intensity);
        if (!_fmodAvailable) return;

        // FMOD call — uncomment when FMOD package is installed:
        // FMODUnity.RuntimeManager.StudioSystem.setParameterByName(_intensityParam, _currentIntensity);

        Debug.Log($"[Audio] Music intensity: {_currentIntensity:F2}");
    }

    // ── SFX ───────────────────────────────────────────────────────────

    /// <summary>Play a one-shot FMOD event at a world position.</summary>
    public void PlaySFX(string eventPath, Vector3 position = default)
    {
        if (!_fmodAvailable) return;
        // FMODUnity.RuntimeManager.PlayOneShot(eventPath, position);
    }

    /// <summary>Play a one-shot FMOD event attached to a Transform.</summary>
    public void PlaySFXAttached(string eventPath, Transform target)
    {
        if (!_fmodAvailable) return;
        // FMODUnity.RuntimeManager.PlayOneShotAttached(eventPath, target.gameObject);
    }

    // ── Dialogue ──────────────────────────────────────────────────────

    public void PlayDialogueLine(string eventPath)
    {
        if (!_fmodAvailable) return;
        // FMODUnity.RuntimeManager.PlayOneShot(eventPath);
    }

    // ── Internal ──────────────────────────────────────────────────────

    private void LoadBanks()
    {
        // FMODUnity.RuntimeManager.LoadBank(_masterBankPath, loadSamples: true);
        // FMODUnity.RuntimeManager.LoadBank(_musicBankPath,  loadSamples: false);
        // FMODUnity.RuntimeManager.LoadBank(_sfxBankPath,    loadSamples: false);
        Debug.Log("[Audio] FMOD banks loaded (stub — install FMOD package to activate).");
    }

    private bool CheckFMOD()
    {
        // Returns true when FMOD assembly is present
        // In pre-production this will always be false
        return false;
    }

    private void OnIntensityChanged(MusicIntensityChangedEvent e)
        => SetMusicIntensity(e.Intensity);

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        // Pause / resume audio engine with game state
        switch (e.Next)
        {
            case GameState.Paused:
                // FMODUnity.RuntimeManager.PauseAllEvents(true);
                break;
            case GameState.Playing:
                // FMODUnity.RuntimeManager.PauseAllEvents(false);
                break;
        }
    }
}
