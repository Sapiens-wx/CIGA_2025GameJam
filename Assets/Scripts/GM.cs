using UnityEngine;

public class GM : MonoBehaviour
{
    public static GM instance;
    
    [Header("Controller References")]
    public CameraController cameraController;
    public BackgroundController backgroundController;
    public CursorManager cursorManager;
    
    [Header("Game State")]
    public int photoCount = 0;
    
    void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
