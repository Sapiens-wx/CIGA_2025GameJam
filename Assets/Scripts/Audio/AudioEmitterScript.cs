using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEmitterScript : MonoBehaviour
{
    [field: SerializeField] StudioEventEmitter emitter;
    // Start is called before the first frame update
    void Awake()
    {

    }
    void Update()
    {
        if (emitter.EventInstance.isValid())
        {
            PLAYBACK_STATE state;
            emitter.EventInstance.getPlaybackState(out state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                AudioManager.instance.CleanEmitter(emitter);
                Destroy(gameObject);
            }
        }
    }

    public void PlayEmitter()
    {
        emitter.Play();
    }
}
