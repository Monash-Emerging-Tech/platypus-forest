using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private float _sceneFadeDuration = 0.5f;

    [SerializeField]
    public InputActionReference inputActionReference_SceneResetAction;

    private SceneFade _sceneFade;
    private bool _isLoading;

    public static SceneController instance { get; private set; }

    private void Awake()
    {
        instance = this;

        _sceneFade = GetComponentInChildren<SceneFade>(true);
        if (_sceneFade == null)
            _sceneFade = FindObjectOfType<SceneFade>();
        if (_sceneFade == null)
            Debug.LogWarning("[SceneController] No SceneFade found in this scene.");
    }

    private void OnEnable()
    {
        if (inputActionReference_SceneResetAction != null)
        {
            inputActionReference_SceneResetAction.action.Enable();
            inputActionReference_SceneResetAction.action.performed += ResetGame;
        }
    }

    private void OnDisable()
    {
        if (inputActionReference_SceneResetAction != null)
            inputActionReference_SceneResetAction.action.performed -= ResetGame;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        StartCoroutine(FadeInScene());
    }

    private IEnumerator FadeInScene()
    {
        if (_sceneFade != null)
            yield return _sceneFade.FadeInCoroutine(_sceneFadeDuration);
    }

    public void ResetGame(InputAction.CallbackContext context)
    {
        LoadScene(0);
    }

    // Request a scene change by BUILD INDEX
    public void LoadScene(int buildIndex)
    {
        if (_isLoading) return;

        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("[SceneController] Build index " + buildIndex + " is not in Build Settings.");
            return;
        }

        _isLoading = true;
        StartCoroutine(LoadSceneCoroutine(buildIndex));
    }

    private IEnumerator LoadSceneCoroutine(int buildIndex)
    {
        if (_sceneFade != null)
            yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);

        SceneManager.LoadScene(buildIndex);
    }
}