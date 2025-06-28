using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SceneController.Instance.LoadScene(SceneType.MainMenu);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SceneController.Instance.LoadScene(SceneType.GamePlay);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SceneController.Instance.LoadScene(SceneType.CGGallery);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SceneController.Instance.LoadScene(SceneType.SpecialScene);
    }
}