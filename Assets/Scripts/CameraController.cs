using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.LowLevelPhysics;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas canvas; 
    public GameObject currentCameraFrame; 
    
    [Header("Photo Display")]
    public GameObject photoDisplayPanel; // Photo display panel
    public Image capturedPhotoImage; // Image to show the captured photo
    public Button photoButton; // Button for photo interaction
    public float slideAnimationDuration = 2.0f; // Duration for slide animation
    
    [Header("Photo Settings")]
    public float zoomScale = 1.25f;
    public float slowMotionFactor = 0.3f;
    public float flashDuration = 0.6f;
    public Image flashOverlay;
    public Image mask;
    public Sprite frameFocus;
    public float focusMaskAlpha = 0.9f;
    public Sprite frameNormal;
    public float normalMaskAlpha = 0.8f;

    [Header("Item Detection")]
    public List<GameObject> photoItems = new List<GameObject>();
    public List<bool> itemsDetected = new List<bool>();

    
    [Header("Screenshot Settings")]
    public string screenshotFolder = "Screenshots";
    public bool saveToDocuments = false; // Save to Documents folder or game directory
    public bool isCameraFrameActive {get; private set;} = false;
    public bool isAiming {get; private set;} = false;
    public bool 
    canTakePhoto {get; private set;} = false;
    
    public float frameWidth {get; private set;}
    public float frameHeight {get; private set;}
    
    private Camera mainCamera; 

    private bool isAnimating = false; 
    private bool isDisplayingPhoto = false;
    private Vector3 originalFrameScale = Vector3.one;
    private Coroutine aimingFrameCoroutine = null;
    private Coroutine aimingBgCoroutine = null;
    private Coroutine aimingMaskCoroutine = null;


    void Start()
    {
        // Initialize main camera
        mainCamera = Camera.main;
        
        // Store original frame scale
        if (currentCameraFrame != null)
        {
            originalFrameScale = Vector3.one;
        }
        
        // Initialize flash overlay
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            flashOverlay.gameObject.SetActive(false);
        }

        // Initialize photo display panel
        if (photoDisplayPanel != null)
        {
            photoDisplayPanel.SetActive(false);
            // Set initial position (below screen)
            RectTransform panelRect = photoDisplayPanel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(0, -panelRect.rect.height);
        }

        // Initialize photo button interaction
        if (photoButton != null)
        {
            photoButton.onClick.AddListener(OnPhotoClicked);
        }

        // Initialize items detected
        itemsDetected.Clear();
        foreach (GameObject item in photoItems)
        {
            itemsDetected.Add(false);
        }
    }

    void Update()
    {   
        // Update frame width and height
        if (currentCameraFrame != null)
        {
            frameWidth = currentCameraFrame.GetComponent<RectTransform>().rect.width * 
            currentCameraFrame.transform.localScale.x;
            frameHeight = currentCameraFrame.GetComponent<RectTransform>().rect.height * 
            currentCameraFrame.transform.localScale.y;
        }

        // Handle photo taking input and effects
        HandlePhotoInput();
        
        // Update camera frame when frame is active
        if (isCameraFrameActive)
        {
            UpdateCameraPosition();
            GM.instance.cursorManager.SetGameMode();
        }
        else
        {
            GM.instance.cursorManager.SetMenuMode();
        }
    }
    
    #region Player Logic
    /// <summary>
    /// Handle photo taking input and effects
    /// </summary>
    private void HandlePhotoInput()
    {

        // Handle photo taking when camera frame is active
        if (isCameraFrameActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartAiming();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (canTakePhoto)
                {
                    TakePhoto();
                }
                else{
                    StopAiming();
                }
            }
        }

        if (isAnimating || isDisplayingPhoto) return;
        
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (!isCameraFrameActive)
            {
                ShowCameraFrame();
            }
            else
            {
                HideCameraFrame();
            }
        }
    }
    
    /// <summary>
    /// Start aiming mode - zoom frame and slow mouse
    /// </summary>
    private void StartAiming()
    {
        isAiming = true;
        currentCameraFrame.GetComponent<Image>().sprite = frameFocus;
        aimingFrameCoroutine = StartCoroutine(AnimateFrameZoom(true));
        BackgroundController bgController = GM.instance.backgroundController;
        aimingBgCoroutine = StartCoroutine(bgController.BackgroundZoom(bgController.focusBackgroundScale));
        aimingMaskCoroutine = StartCoroutine(MaskDarken(true));
    }

    private void StopAiming()
    {
        isAiming = false;
        if (aimingFrameCoroutine != null)
        {
            StopCoroutine(aimingFrameCoroutine);
            aimingFrameCoroutine = StartCoroutine(AnimateFrameZoom(false));
        }
        if (aimingBgCoroutine != null)
        {
            StopCoroutine(aimingBgCoroutine);
            aimingBgCoroutine = StartCoroutine(GM.instance.backgroundController.
                BackgroundZoom(GM.instance.backgroundController.cameraBackgroundScale));
        }
        if (aimingMaskCoroutine != null)
        {
            StopCoroutine(aimingMaskCoroutine);
            aimingMaskCoroutine = StartCoroutine(MaskDarken(false));
        }
    }
    
    /// <summary>
    /// Take photo - flash effect and reset
    /// </summary>
    private void TakePhoto()
    {
        isAiming = false;
        canTakePhoto = false;
        isDisplayingPhoto = true;
        
        // Detect items in camera frame
        DetectItemsInFrame();
        foreach (bool detected in itemsDetected)
        {
            Debug.Log($"Detected: {detected}");
        }
        
        // Capture the screenshot first and then start the animations
        StartCoroutine(CaptureAndThenAnimate());
    }
    
    /// <summary>
    /// Capture screenshot then start animations
    /// </summary>
    private IEnumerator CaptureAndThenAnimate()
    {
        // Capture the screenshot first
        yield return StartCoroutine(CaptureScreenshot());
        // Then start all the exit animations
        HideCameraFrame();
        StartCoroutine(AnimateFrameZoom(false));
        StartCoroutine(FlashEffect());
        if (photoDisplayPanel != null)
        {
            yield return new WaitForSeconds(1f);
            ShowCapturedPhoto();
        }
        else
        {
            GM.instance.backgroundController.background.SetActive(false);
            yield return new WaitForSeconds(2f);
            SceneController.Instance.LoadScene(SceneType.GamePlay1);
        }
        yield break;
    }
    #endregion
        
    #region Camera Logic


    /// <summary>
    /// Show camera frame
    /// </summary>
    private void ShowCameraFrame()
    {
        if (canvas != null)
        {
            currentCameraFrame.GetComponent<Image>().sprite = frameNormal;
            
            // Set initial position
            currentCameraFrame.GetComponent<RectTransform>().position = Input.mousePosition;
            
            // Start animation
            StartCoroutine(AnimateCameraIn());
            StartCoroutine(SetPlayerSprite(false));
            StartCoroutine(SetPlayerSprite(false));
            BackgroundController bgController = GM.instance.backgroundController;
            StartCoroutine(bgController.BackgroundZoom( bgController.cameraBackgroundScale));

            // Set camera frame active
            isCameraFrameActive = true;
        }
    }

    /// <summary>
    /// Hide camera frame
    /// </summary>
    private void HideCameraFrame()
    {
        if (currentCameraFrame != null)
        {
            // Reset photo taking state
            isAiming = false;
            
            // Start fade out animation
            StartCoroutine(AnimateCameraOut());
            StartCoroutine(SetPlayerSprite(true));
            StartCoroutine(SetPlayerSprite(true));
            BackgroundController bgController = GM.instance.backgroundController;
            StartCoroutine(bgController.BackgroundZoom(bgController.originalBackgroundScale));
            isCameraFrameActive = false;
        }
    }
    
    /// <summary>
    /// Update camera frame position using mouse movement
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (currentCameraFrame != null && canvas != null)
        {
            // Get mouse delta 
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
            // Apply mouse sensitivity reduction when taking photo
            if (isAiming)
            {
                mouseDelta *= slowMotionFactor;
            }
            
            // Scale mouse delta for UI movement
            float mouseSensitivity = 100f;
            mouseDelta *= mouseSensitivity;
            
            // Get frame rect transform
            RectTransform frameRect = currentCameraFrame.GetComponent<RectTransform>();
            
            // Update frame position based on mouse delta
            Vector3 newPosition = frameRect.localPosition + new Vector3(mouseDelta.x, mouseDelta.y, 0);
            
            // Get confined area from cursor manager
            Rect confinedArea = GetConfinedArea(frameWidth, frameHeight);
            
            // Clamp position to confined bounds
            newPosition.x = Mathf.Clamp(newPosition.x, -confinedArea.width/2, confinedArea.width/2);
            newPosition.y = Mathf.Clamp(newPosition.y, -confinedArea.height/2, confinedArea.height/2);
            
            // Set photo frame position
            frameRect.localPosition = newPosition;
        }
    }
    
    /// <summary>
    /// Get confined area for camera frame
    /// </summary>
    public Rect GetConfinedArea(float width, float height)
    {
        Rect confinedArea = new Rect(
            width / 2,
            height / 2,
            canvas.GetComponent<RectTransform>().rect.width - width,
            canvas.GetComponent<RectTransform>().rect.height - height
        );
        return confinedArea;
    }

    #endregion

    #region Screenshot Logic

    /// <summary>
    /// Capture main camera content 
    /// 泥模模的我也不知道咋写的但能跑
    /// </summary>
    private IEnumerator CaptureScreenshot()
    {
        // Wait for end of frame to ensure proper rendering
        yield return new WaitForEndOfFrame();

        // Get currentCameraFrame rect transform
        RectTransform cameraFrameRect = currentCameraFrame.GetComponent<RectTransform>();
        Canvas parentCanvas = currentCameraFrame.GetComponentInParent<Canvas>();
        
        // Get screen dimensions
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Create RenderTexture for camera rendering
        RenderTexture renderTexture = new RenderTexture(screenWidth, screenHeight, 24);
        RenderTexture previousTarget = mainCamera.targetTexture;
        
        // Set camera to render to our texture
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        // Set active render texture to read from
        RenderTexture.active = renderTexture;

        // Convert UI rect to screen coordinates
        Vector3[] worldCorners = new Vector3[4];
        cameraFrameRect.GetWorldCorners(worldCorners);
        
        // Get screen bounds using RectTransformUtility for UI elements
        Vector2 screenMin, screenMax;
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For screen space overlay, world corners are already in screen space
            screenMin = worldCorners[0];
            screenMax = worldCorners[2];
        }
        else
        {
            // For screen space camera or world space
            screenMin = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera ?? mainCamera, worldCorners[0]);
            screenMax = RectTransformUtility.WorldToScreenPoint(parentCanvas.worldCamera ?? mainCamera, worldCorners[2]);
        }
        
        // Calculate crop rectangle in camera render
        int x = Mathf.FloorToInt(screenMin.x);
        int y = Mathf.FloorToInt(screenMin.y);
        int width = Mathf.FloorToInt(screenMax.x - screenMin.x);
        int height = Mathf.FloorToInt(screenMax.y - screenMin.y);
        
        // Clamp to render texture bounds
        x = Mathf.Clamp(x, 0, screenWidth);
        y = Mathf.Clamp(y, 0, screenHeight);
        width = Mathf.Clamp(width, 1, screenWidth - x);
        height = Mathf.Clamp(height, 1, screenHeight - y);

        // Create texture and read pixels from camera render
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(x, y, width, height), 0, 0);
        screenshotTexture.Apply();

        // Restore camera settings
        mainCamera.targetTexture = previousTarget;
        RenderTexture.active = null;

        // Save the screenshot
        SaveScreenshot(screenshotTexture);
        
        // Set captured photo to UI Image
        SetCapturedPhotoToUI(screenshotTexture);

        // Clean up render texture only (keep screenshot texture for UI)
        Destroy(renderTexture);
    }
    
    /// <summary>
    /// Save screenshot texture to file
    /// </summary>
    private void SaveScreenshot(Texture2D texture)
    {
        if (texture == null) return;
        
        // Generate filename
        string filename = $"Photo_{GM.instance.photoCount}.png";
        GM.instance.photoCount++;
        
        // Determine save path
        string savePath = GetScreenshotPath();
        print("savePath: " + savePath);
        
        // Ensure directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Full file path
        string fullPath = Path.Combine(savePath, filename);
        
        // Convert to PNG and save
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);
        
        Debug.Log($"Screenshot saved: {fullPath}");
    }
    
    /// <summary>
    /// Get the path where screenshots should be saved
    /// </summary>
    private string GetScreenshotPath()
    {
        if (saveToDocuments)
        {
            // Save to Documents/GameName/Screenshots
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, Application.productName, screenshotFolder);
        }
        else
        {
            // Save to game directory/Screenshots
            return Path.Combine(Application.dataPath, "..", screenshotFolder);
        }
    }
    #endregion

    #region Detecting Logic
    
    /// <summary>
    /// Check which items are within camera frame bounds
    /// </summary>
    public void DetectItemsInFrame()
    {
        
        // Get camera frame world corners
        RectTransform frameRect = currentCameraFrame.GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        frameRect.GetWorldCorners(worldCorners);
        
        // Get frame bounds in screen space
        Vector2 frameMin = worldCorners[0];
        Vector2 frameMax = worldCorners[2];
        print($"frameMin: {frameMin}, frameMax: {frameMax}");
        
        // Check each item
        foreach (GameObject item in photoItems)
        {
            // Convert item world position to screen position 
            Vector3 itemScreenPos = mainCamera.WorldToScreenPoint(item.transform.position);
            
            if (itemScreenPos.x >= frameMin.x && itemScreenPos.x <= frameMax.x &&
                itemScreenPos.y >= frameMin.y && itemScreenPos.y <= frameMax.y)
            {
                itemsDetected[photoItems.IndexOf(item)] = true;
            }
            else
            {
                itemsDetected[photoItems.IndexOf(item)] = false;
            }
        }
    }
    
    #endregion

    #region Photo Display Logic
    
    /// <summary>
    /// Show captured photo with slide animation
    /// </summary>
    private void ShowCapturedPhoto()
    {
        // Activate panel
        photoDisplayPanel.SetActive(true);
        
        // Start slide animation
        StartCoroutine(SlidePhotoIn());
    }
    
    /// <summary>
    /// Slide photo panel from bottom
    /// </summary>
    private IEnumerator SlidePhotoIn()
    {
        if (photoDisplayPanel == null) yield break;
        
        RectTransform panelRect = photoDisplayPanel.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(0, -panelRect.rect.height);
        Vector2 endPos = new Vector2(0, 0); // Show partially
        
        float elapsed = 0f;
        
        while (elapsed < slideAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideAnimationDuration;
            
            // Smooth easing
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);
            
            yield return null;
        }
        
        panelRect.anchoredPosition = endPos;
    }
    
    /// <summary>
    /// Hide photo panel with slide animation
    /// </summary>
    private IEnumerator SlidePhotoOut()
    {
        if (photoDisplayPanel == null) yield break;
        
        RectTransform panelRect = photoDisplayPanel.GetComponent<RectTransform>();
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 endPos = new Vector2(0, -panelRect.rect.height);
        
        float elapsed = 0f;
        
        while (elapsed < slideAnimationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (slideAnimationDuration * 0.5f);
            
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            
            yield return null;
        }
        
        panelRect.anchoredPosition = endPos;
        photoDisplayPanel.SetActive(false);
        isDisplayingPhoto = false;
    }
    
    /// <summary>
    /// Set captured photo to UI Image
    /// </summary>
    private void SetCapturedPhotoToUI(Texture2D photoTexture)
    {
        if (capturedPhotoImage == null || photoTexture == null) return;
        
        // Create sprite from texture
        Sprite photoSprite = Sprite.Create(
            photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        
        // Set sprite to image
        capturedPhotoImage.sprite = photoSprite;
    }
    
    /// <summary>
    /// Handle photo click interaction
    /// </summary>
    private void OnPhotoClicked()
    {
        // Hide photo panel
        StartCoroutine(SlidePhotoOut());
        /* 
            * important: only set isAnimating to false when player click photo button
            * otherwise, no player action allowed   
        */
        isAnimating = false;
    }
    
    #endregion

    #region Animations
    /// <summary>
    /// Camera frame fade in animation
    /// </summary>
    private IEnumerator AnimateCameraIn()
    {
        if (currentCameraFrame == null) yield break;
        
        isAnimating = true;
        
        CanvasGroup canvasGroup = currentCameraFrame.GetComponent<CanvasGroup>();
        Transform frameTransform = currentCameraFrame.transform;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Fade in
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            
            // Scale up 
            float scale = Mathf.Lerp(0.5f, 1f, Mathf.SmoothStep(0f, 1f, progress));
            frameTransform.localScale = originalFrameScale * scale;
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        frameTransform.localScale = originalFrameScale;
        
        isAnimating = false;
    }
    
    /// <summary>
    /// Camera frame fade out animation
    /// </summary>
    private IEnumerator AnimateCameraOut()
    {
        if (currentCameraFrame == null) yield break;
        
        isAnimating = true;

        CanvasGroup canvasGroup = currentCameraFrame.GetComponent<CanvasGroup>();
        Transform frameTransform = currentCameraFrame.transform;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Fade out
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            // Scale down
            float scale = Mathf.Lerp(1f, 0.5f, Mathf.SmoothStep(0f, 1f, progress));
            frameTransform.localScale = originalFrameScale * scale;
            
            yield return null;
        }
        canvasGroup.alpha = 0f;
        frameTransform.localScale = Vector3.zero;
        isCameraFrameActive = false;
        
        isAnimating = false;
    }
    
    /// <summary>
    /// Create white flash effect when taking photo
    /// </summary>
    private IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;

        flashOverlay.gameObject.SetActive(true);
        
        float flashInDuration = 0.1f;
        float elapsed = 0f;
        
        // Quick fade in
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / flashInDuration;
            
            float alpha = Mathf.Lerp(0f, 0.8f, progress);
            flashOverlay.color = new Color(1f, 1f, 1f, alpha);
            
            yield return null;
        }
        
        // Hold white for a moment
        flashOverlay.color = new Color(1f, 1f, 1f, 0.8f);
        yield return new WaitForSeconds(0.1f);
        
        // Gradual fade out
        float fadeOutDuration = flashDuration - 0.2f;
        elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            
            float alpha = Mathf.Lerp(0.8f, 0f, progress);
            flashOverlay.color = new Color(1f, 1f, 1f, alpha);
            
            yield return null;
        }
        
        flashOverlay.color = new Color(1f, 1f, 1f, 0f);
        flashOverlay.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Animate camera frame zoom in(aiming) 
    /// and zoom out(after taking photo)
    /// </summary>
    private IEnumerator AnimateFrameZoom(bool isZoomIn)
    {
        // Get frame transform and scale
        Transform frameTransform = currentCameraFrame.transform;
        Vector3 startScale = frameTransform.localScale;
        Vector3 targetScale = isZoomIn ? originalFrameScale * zoomScale : originalFrameScale;
        isAnimating = true;
        float duration = 0.5f;
        float elapsed = 0f;
        
        // Animate frame 
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            frameTransform.localScale = Vector3.Lerp(startScale, targetScale, 
                Mathf.SmoothStep(0f, 1f, progress));
            
            yield return null;
        }
        
        frameTransform.localScale = targetScale;
        isAnimating = false;
        if (isZoomIn)
        {
            canTakePhoto = true;
        }
    }

    /// <summary>
    /// Darken mask when aiming
    /// </summary>
    private IEnumerator MaskDarken(bool isDarken)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float startAlpha = mask.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            float alpha = Mathf.Lerp(startAlpha, isDarken ? focusMaskAlpha : normalMaskAlpha, progress);
            mask.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        mask.color = new Color(0f, 0f, 0f, isDarken ? focusMaskAlpha : normalMaskAlpha);
    }
    
    /// <summary>
    /// Set player sprite to show or hide
    /// </summary>
    private IEnumerator SetPlayerSprite(bool isShow)
    {
        if (GM.instance.backgroundController.playerSprite == null) yield break;
        float duration = 0.5f;
        float elapsed = 0f;
        float startAlpha = GM.instance.backgroundController.playerSprite.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, isShow ? 1f : 0f, progress);
            GM.instance.backgroundController.playerSprite.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        GM.instance.backgroundController.playerSprite.color = new Color(1f, 1f, 1f, isShow ? 1f : 0f);
    }
    #endregion

}