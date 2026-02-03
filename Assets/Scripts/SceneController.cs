using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    // Duration used for scene fade in/out
    [SerializeField]
    private float _sceneFadeDuration;

    private SceneFade _sceneFade;

    private void Awake()
    {
        // Find the SceneFade component's children
        _sceneFade = GetComponentInChildren<SceneFade>();
    }

    private IEnumerator Start()
    {
        // Fade in the scene when it first loads and run automatically
        yield return (_sceneFade.FadeInCoroutine(_sceneFadeDuration));
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
        yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);

        // Begin asynchronous loading of the target scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Load the target scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
