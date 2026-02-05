using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    // Duration used for scene fade in/out
    [SerializeField]
    private float _sceneFadeDuration;

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
        
        // Find the SceneFade component's children
        _sceneFade = GetComponentInChildren<SceneFade>();
        
        if (_sceneFade != null)
        {
            // Also make the SceneFade persist
            DontDestroyOnLoad(_sceneFade.gameObject);
        }
    }

    private void OnEnable()
    {
        // Follow to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unfollow from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in when a new scene loads
        StartCoroutine(FadeInScene());
    }

    private IEnumerator FadeInScene()
    {
        if (_sceneFade != null)
        {
            yield return _sceneFade.FadeInCoroutine(_sceneFadeDuration);
        }
    }

    // Public method to request a scene change
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    // Handles fading out and loading the new scene
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Fade the screen to black before changing scenes
        if (_sceneFade != null)
        {
            yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);
        }

        // Load the target scene
        SceneManager.LoadScene(sceneName);
    }
}