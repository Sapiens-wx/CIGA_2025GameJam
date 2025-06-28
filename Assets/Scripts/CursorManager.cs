using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private Rect confinedArea;
    
    void Start()
    {
        LockCursor();
        HideCursor();
    }
    
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void HideCursor()
    {
        Cursor.visible = false;
    }
    public void ShowCursor()
    {
        Cursor.visible = true;
    }

    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
            ShowCursor();
        }
        else
        {
            LockCursor();
            HideCursor();
        }
    }

    public void SetGameMode()
    {
        LockCursor();
        HideCursor();
    }

    public void SetMenuMode()
    {
        UnlockCursor();
        ShowCursor();
    }

    public bool IsLocked()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public bool IsVisible()
    {
        return Cursor.visible;
    }
}