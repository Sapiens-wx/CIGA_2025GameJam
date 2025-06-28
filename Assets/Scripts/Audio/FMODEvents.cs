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
    [field: SerializeField] public EventReference bookUI;
    [field: SerializeField] public EventReference cameraTakePic;
    [field: SerializeField] public EventReference cameraPrint;
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
