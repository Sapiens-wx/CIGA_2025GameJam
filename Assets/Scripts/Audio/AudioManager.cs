using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum SoundByScene
{
    Menu,
    Game,
    CG
}

public enum MusicIndex
{
    Empty,
    Menu
}

public enum AmbienceIndex
{ 
    Empty,
    Room
}
public enum BookState
{
    Open,
    Close,
    FlipNext,
    FlipPrev
}

public enum PrintState
{
    Correct,
    Wrong
}

public enum AudioCGType
{
    Red,
    Blue, 
    Purple,
    Apple
}

public class AudioManager : MonoBehaviour
{
    [Header("Scene Parameters")]
    [SerializeField] private SoundByScene currentSceneForSound;

    [Header("Prefabs")]
    [SerializeField] private GameObject audioEmitter;

    [Header("FMOD Parameters")]
    private PARAMETER_ID musicIndex;
    private PARAMETER_ID ambienceIndex;
    private PARAMETER_ID bookState;
    private PARAMETER_ID cameraFocus;
    
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;

    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        SetFMODParameterIDs();

        InitializeAmbience(FMODEvents.instance.ambience);
        InitializeMusic(FMODEvents.instance.music);
        ambienceEventInstance.start();
        musicEventInstance.start();

        switch(currentSceneForSound)
        {
            case SoundByScene.Menu:
                SetAmbienceIndex(AmbienceIndex.Empty);
                SetMusicIndex(MusicIndex.Menu);
                break;
            case SoundByScene.Game:
                SetAmbienceIndex(AmbienceIndex.Room);
                SetMusicIndex(MusicIndex.Empty);
                break;
            case SoundByScene.CG:
                SetAmbienceIndex(AmbienceIndex.Empty);
                SetMusicIndex(MusicIndex.Empty);
                break;
        }
    }

    //for testing
    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(AudioCameraEnter());
        }
        else if(Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(AudioCameraExit());
        }
        else if(Input.GetKeyDown(KeyCode.E))
        {
            AudioCameraZoomIn();
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            AudioCameraZoomOut();
        }
        else if(Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(AudioCameraTakePic());
        }
        else if(Input.GetKeyDown(KeyCode.Y))
        {
            AudioCameraKeepPic();
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            AudioOpenBook();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            AudioCloseBook();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            AudioNextPage();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            AudioPrevPage();
        }*/
    }


    private void SetFMODParameterIDs()
    {
        EventDescription event_desc;
        PARAMETER_DESCRIPTION para_desc;

        RuntimeManager.StudioSystem.getEvent("event:/music/music", out event_desc);
        event_desc.getParameterDescriptionByName("music_index", out para_desc);
        musicIndex = para_desc.id;

        RuntimeManager.StudioSystem.getEvent("event:/env/amb", out event_desc);
        event_desc.getParameterDescriptionByName("ambience_index", out para_desc);
        ambienceIndex = para_desc.id;

        RuntimeManager.StudioSystem.getEvent("event:/book/book_ui", out event_desc);
        event_desc.getParameterDescriptionByName("book_state", out para_desc);
        bookState = para_desc.id;

        RuntimeManager.StudioSystem.getEvent("event:/env/amb", out event_desc);
        event_desc.getParameterDescriptionByName("camera_focus", out para_desc);
        cameraFocus = para_desc.id;
        //add more parameter ids here.
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
    }

    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateInstance(ambienceEventReference);
    }
    public void SetMusicIndex(MusicIndex index)
    {
        musicEventInstance.setParameterByID(musicIndex, (float)index);
    }

    public void SetAmbienceIndex(AmbienceIndex index)
    {
        ambienceEventInstance.setParameterByID(ambienceIndex, (float)index);
    }

    public StudioEventEmitter CreateEventEmitterObject(EventReference eventReference, Transform parent)
    {
        GameObject _inst = Instantiate(audioEmitter, parent);
        StudioEventEmitter _emitter = _inst.GetComponent<StudioEventEmitter>();

        _emitter.EventReference = eventReference;

        _emitter.Play();

        eventEmitters.Add(_emitter);

        return _emitter;
    }

    public StudioEventEmitter CreateEventEmitterObject(EventReference eventReference, Transform parent, PARAMETER_ID id, float setVar)
    {
        GameObject _inst = Instantiate(audioEmitter, parent);
        StudioEventEmitter _emitter = _inst.GetComponent<StudioEventEmitter>();

        _emitter.EventReference = eventReference;

        _emitter.Play();
        
        eventEmitters.Add(_emitter);

        StartCoroutine(SetEmitterParameterNextFrame(_emitter, id, setVar));

        return _emitter;
    }

    private IEnumerator SetEmitterParameterNextFrame(StudioEventEmitter emitter, PARAMETER_ID id, float setVar)
    {
        yield return null;
        emitter.EventInstance.setParameterByID(id, setVar);
    }

    public void AudioPrevPage()
    {
        CreateEventEmitterObject(FMODEvents.instance.bookFlipPrev, this.transform);
    }

    public void AudioNextPage()
    {
        CreateEventEmitterObject(FMODEvents.instance.bookFlipNext, this.transform);
    }

    public void AudioOpenBook()
    {
        CreateEventEmitterObject(FMODEvents.instance.bookOpen, this.transform);
    }

    public void AudioCloseBook()
    {
        CreateEventEmitterObject(FMODEvents.instance.bookClose, this.transform);
    }

    public IEnumerator AudioCameraEnter()
    {
        float timer = 0f;
        float totalTime = 0.4f;

        CreateEventEmitterObject(FMODEvents.instance.cameraEnter, this.transform);

        while(timer < totalTime)
        {
            ambienceEventInstance.setParameterByID(cameraFocus, (float)(timer / totalTime));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator AudioCameraExit()
    {
        float timer = 0f;
        float totalTime = 0.4f;

        CreateEventEmitterObject(FMODEvents.instance.cameraExit, this.transform);

        while (timer < totalTime)
        {
            ambienceEventInstance.setParameterByID(cameraFocus, (1-(timer / totalTime)));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void AudioCameraZoomIn()
    {
        CreateEventEmitterObject(FMODEvents.instance.cameraZoomIn, this.transform);
    }

    public void AudioCameraZoomOut()
    {
        CreateEventEmitterObject(FMODEvents.instance.cameraZoomOut, this.transform);
    }

    public IEnumerator AudioCameraTakePic()
    {
        float timer = 0f;
        float totalTime = 0.25f;

        CreateEventEmitterObject(FMODEvents.instance.cameraTakePic, this.transform);

        while (timer < totalTime)
        {
            ambienceEventInstance.setParameterByID(cameraFocus, (1 - (timer / totalTime)));
            timer += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public void AudioCameraKeepPic()
    {
        CreateEventEmitterObject(FMODEvents.instance.cameraKeepPic, this.transform);
    }

    public void AudioPlayCG(AudioCGType type)
    {
        switch (type)
        {
            case AudioCGType.Red:
                CreateEventEmitterObject(FMODEvents.instance.cgRed, this.transform);
                break;
            case AudioCGType.Blue:
                CreateEventEmitterObject(FMODEvents.instance.cgBlue, this.transform);
                break;
            case AudioCGType.Purple:
                CreateEventEmitterObject(FMODEvents.instance.cgPurple, this.transform);
                break;
            case AudioCGType.Apple:
                CreateEventEmitterObject(FMODEvents.instance.cgApple, this.transform);
                break;
        }
    }

    public void CleanEmitter(StudioEventEmitter emitter)
    {
        eventEmitters.Remove(emitter);
    }

    private void CleanUp()
    {
        // stop and release any created instances
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        // stop all of the event emitters, because if we don't they may hang around in other scenes
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
