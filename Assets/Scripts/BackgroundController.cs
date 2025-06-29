using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BackgroundController: MonoBehaviour
{
    [Header("Background Settings")]
    public GameObject background;
    public SpriteRenderer playerSprite;
    public float maxTiltAngle = 15f;
    public float tiltSpeed = 5f;
    public float shiftSpeed = 5f;
    
    [Header("Photo Zoom Settings")]
    public Vector3 originalBackgroundScale = Vector3.one;
    public Vector3 cameraBackgroundScale = Vector3.one * 1.2f;
    public Vector3 focusBackgroundScale = Vector3.one * 1.5f;
    
    public float scaleFactorX = 1f;
    public float scaleFactorY = 1f;
    private Vector3 originalBackgroundPosition;
    private Vector3 targetTilt;
    private Vector3 targetShift;
    
    void Start()
    {
        // Store original background position and scale
        if (background != null)
        {
            originalBackgroundPosition = background.transform.position;
        }
    }

    void Update()
    {
        // Track target background tilt and shift by mouse position when camera frame is active
        if (GM.instance.cameraController.isCameraFrameActive)
        {
            (targetTilt, targetShift) = TargetBG();
        }
        // Reset target background when camera frame is not active
        else
        {
            targetTilt = Vector3.zero;
            targetShift = originalBackgroundPosition;
        }

        // Smoothly interpolate to target values
        background.transform.rotation = Quaternion.Lerp(background.transform.rotation, 
            Quaternion.Euler(targetTilt), Time.deltaTime * tiltSpeed);
        background.transform.position = Vector3.Lerp(background.transform.position, 
            targetShift, Time.deltaTime * shiftSpeed);
    }
    
    #region Photo Zoom Methods
    public IEnumerator BackgroundZoom(Vector3 targetScale)
    {
        // Get frame transform and scale
        Transform backgroundTransform = background.transform;
        Vector3 startScale = backgroundTransform.localScale;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        // Animate frame 
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, 
                Mathf.SmoothStep(0f, 1f, progress));
            backgroundTransform.localScale = currentScale;
            yield return null;
        }
        
        backgroundTransform.localScale = targetScale;

    }
    #endregion

    #region Background Logic
    /// <summary>
    /// Track target background tilt and shift by mouse position
    /// </summary>
    public (Vector3, Vector3) TargetBG()
    {
        Vector2 framePosition = GM.instance.cameraController.currentCameraFrame.transform.position;
        
        Vector3 targetTilt = CalcBGTilt(framePosition, maxTiltAngle);
        Vector3 targetShift = originalBackgroundPosition + CalcBgShift(framePosition, 
        (background.transform.localScale.x-1) * scaleFactorX,
        (background.transform.localScale.y-1) * scaleFactorY);

        return (targetTilt, targetShift);
    }

    /// <summary>
    /// Calculate background tilt
    /// </summary>
    public Vector3 CalcBGTilt(Vector2 mousePosition, float maxTiltAngle)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
        // Calculate offset from center
        Vector2 offset = (mousePosition - screenCenter);
        offset.x /= Screen.width * 0.5f;
        offset.y /= Screen.height * 0.5f;
        
        // Calculate tilt angles
        float tiltX = -offset.y * maxTiltAngle;
        float tiltY = offset.x * maxTiltAngle;

        return new Vector3(tiltX, tiltY, 0);
    }
    
    /// <summary>
    /// Calculate background shift
    /// </summary>
    public Vector3 CalcBgShift(Vector2 mousePosition, float maxHorizontalOffset, float maxVerticalOffset)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
        // Calculate offset from center
        Vector2 offset = (mousePosition - screenCenter);
        float frameWidth = GM.instance.cameraController.frameWidth * Screen.width 
            / GM.instance.cameraController.canvas.GetComponent<RectTransform>().rect.width;
        float frameHeight = GM.instance.cameraController.frameHeight * Screen.height 
            / GM.instance.cameraController.canvas.GetComponent<RectTransform>().rect.height;
        offset.x /= (Screen.width-frameWidth) * 0.5f;
        offset.y /= (Screen.height-frameHeight) * 0.5f;

        // Calculate position 
        float posX = -offset.x * maxHorizontalOffset;
        float posY = -offset.y * maxVerticalOffset;
        return new Vector3(posX, posY, 0);
    }   
    #endregion
}