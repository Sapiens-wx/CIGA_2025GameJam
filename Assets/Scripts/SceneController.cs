using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private SceneType initialScene = SceneType.MainMenu;

    private Dictionary<SceneType, string> sceneNames = new Dictionary<SceneType, string>
{
{ SceneType.MainMenu, "MainMenu" },
{ SceneType.GamePlay, "GamePlay" },
{ SceneType.CGGallery, "CGGallery" },
{ SceneType.SpecialScene, "SpecialScene" }
};

    public int CurrentDay { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadScene(initialScene);
    }

    public void LoadScene(SceneType scene, bool async = true)
    {
        if (scene == SceneType.GamePlay)
        {
            CurrentDay++;
            Debug.Log($"进入 Gameplay，第 {CurrentDay} 天");
        }

        string sceneName = sceneNames[scene];

        if (async)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}