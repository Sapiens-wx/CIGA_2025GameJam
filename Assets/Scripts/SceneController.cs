using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private SceneType initialScene = SceneType.MainMenu;

    private Dictionary<SceneType, string> sceneNames = new Dictionary<SceneType, string>{
        { SceneType.MainMenu, "MainMenu" },

        { SceneType.GamePlay1, "GamePlay1" },

        { SceneType.GamePlay2, "GamePlay2" },
        
        { SceneType.GamePlay3, "GamePlay3" },
        
        { SceneType.GamePlay4, "GamePlay4" },
        
        { SceneType.CG1, "CG1" },
        
        { SceneType.CG4, "CG2" },
        
        { SceneType.CG3, "CG3" },
        
        { SceneType.CG4, "CG4" },
        
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



    public void LoadScene(SceneType scene, bool async = true)
    {
        if (scene == SceneType.GamePlay1 || scene == SceneType.GamePlay2 || scene == SceneType.GamePlay3 || scene == SceneType.GamePlay4)
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