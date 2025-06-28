using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum SoundByScene
{
    Menu,
    Game
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
    private PARAMETER_ID printState;


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
        }
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

        RuntimeManager.StudioSystem.getEvent("event:/camera/camera_print", out event_desc);
        event_desc.getParameterDescriptionByName("print_state", out para_desc);
        printState = para_desc.id;
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

    public GameObject CreateEventEmitterObject(EventReference eventReference, Transform parent)
    {
        GameObject _inst = Instantiate(audioEmitter, parent);
        StudioEventEmitter _emitter = _inst.GetComponent<StudioEventEmitter>();

        _emitter.EventReference = eventReference;

        _emitter.Play();

        eventEmitters.Add(_emitter);

        return _inst;
    }

    public GameObject CreateEventEmitterObject(EventReference eventReference, Transform parent, PARAMETER_ID id, float setVar)
    {
        GameObject _inst = Instantiate(audioEmitter, parent);
        StudioEventEmitter _emitter = _inst.GetComponent<StudioEventEmitter>();

        _emitter.EventReference = eventReference;

        _emitter.Play();

        eventEmitters.Add(_emitter);

        StartCoroutine(SetEmitterParameterNextFrame(_emitter, id, setVar));

        return _inst;
    }

    private IEnumerator SetEmitterParameterNextFrame(StudioEventEmitter emitter, PARAMETER_ID id, float setVar)
    {
        yield return null;
        emitter.EventInstance.setParameterByID(id, setVar);
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
}
