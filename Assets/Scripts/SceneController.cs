using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [SerializeField]
    private float _sceneFadeDuration = 0.5f;

    private SceneFade _sceneFade;

    private void Awake()
    {
        // Singleton pattern - only one SceneController should exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find the SceneFade component in children
        _sceneFade = GetComponentInChildren<SceneFade>();

        if (_sceneFade != null)
        {
            // Optional: if your SceneFade is a separate child GameObject you want persistent
            DontDestroyOnLoad(_sceneFade.gameObject);
        }
        else
        {
            Debug.LogWarning("[SceneController] No SceneFade found in children.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeInScene());
    }

    private IEnumerator FadeInScene()
    {
        if (_sceneFade != null)
        {
            yield return _sceneFade.FadeInCoroutine(_sceneFadeDuration);
        }
    }

    // Request a scene change by BUILD INDEX
    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadSceneCoroutine(buildIndex));
    }

    // Handles fading out and loading the new scene by BUILD INDEX
    private IEnumerator LoadSceneCoroutine(int buildIndex)
    {
        // Fade out before changing scenes
        if (_sceneFade != null)
        {
            yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);
        }

        // Load the target scene
        SceneManager.LoadScene(buildIndex);
    }
}