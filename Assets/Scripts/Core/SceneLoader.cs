using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles async scene loading. Listens for SceneLoadRequestedEvent and
/// loads scenes additively or exclusively. Publishes SceneLoadedEvent on
/// completion so other systems can react.
///
/// Always load Bootstrap first. Never open chapter scenes directly.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private string _loadingSceneName = "LoadingScreen";
    [SerializeField] private float _minimumLoadTime = 0.5f;

    private string _currentScene;
    private bool _isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => EventBus.Subscribe<SceneLoadRequestedEvent>(OnLoadRequested);
    private void OnDisable() => EventBus.Unsubscribe<SceneLoadRequestedEvent>(OnLoadRequested);

    private void OnLoadRequested(SceneLoadRequestedEvent e)
    {
        if (_isLoading) return;
        StartCoroutine(e.Additive ? LoadAdditive(e.SceneName) : LoadExclusive(e.SceneName));
    }

    private IEnumerator LoadExclusive(string sceneName)
    {
        _isLoading = true;

        // Show loading screen additively while unloading current
        if (!string.IsNullOrEmpty(_currentScene))
        {
            SceneManager.LoadScene(_loadingSceneName, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.1f);
        }

        var startTime = Time.realtimeSinceStartup;
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // Enforce minimum load time to prevent jarring transitions
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < _minimumLoadTime)
            yield return new WaitForSeconds(_minimumLoadTime - elapsed);

        op.allowSceneActivation = true;
        yield return op;

        _currentScene = sceneName;
        _isLoading = false;
        EventBus.Publish(new SceneLoadedEvent(sceneName));
    }

    private IEnumerator LoadAdditive(string sceneName)
    {
        _isLoading = true;
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return op;
        _isLoading = false;
        EventBus.Publish(new SceneLoadedEvent(sceneName));
    }

    public void LoadScene(string sceneName, bool additive = false)
        => EventBus.Publish(new SceneLoadRequestedEvent(sceneName, additive));
}
