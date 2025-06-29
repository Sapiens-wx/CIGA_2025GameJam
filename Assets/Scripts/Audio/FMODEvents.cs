using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference music;

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience;

    [field: Header("SFX")]
    [field: SerializeField] public EventReference bookFlipNext;
    [field: SerializeField] public EventReference bookFlipPrev;
    [field: SerializeField] public EventReference bookOpen;
    [field: SerializeField] public EventReference bookClose;
    [field: SerializeField] public EventReference cameraEnter;
    [field: SerializeField] public EventReference cameraExit;
    [field: SerializeField] public EventReference cameraZoomIn;
    [field: SerializeField] public EventReference cameraZoomOut;
    [field: SerializeField] public EventReference cameraTakePic;
    [field: SerializeField] public EventReference cameraKeepPic;
    [field: SerializeField] public EventReference cameraTakePicMenu;

    [field: Header("CG")]
    [field: SerializeField] public EventReference cgBlue;
    [field: SerializeField] public EventReference cgRed;
    [field: SerializeField] public EventReference cgPurple;
    [field: SerializeField] public EventReference cgApple;
    public static FMODEvents instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}
