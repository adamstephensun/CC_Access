using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    // Controls the timing and playing of the audio description clips

    public List<AudioClip> clips = new List<AudioClip>();

    AudioSource source;

    int currentClip = 0;
    bool silent = false;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayNextClip(){
        currentClip++;

        //check currentClip is in bounds
        source.clip = clips[currentClip];
        source.Play();
    }

    public void PlayClip(int index){
        // check index is in bounds
        source.clip = clips[index];
        source.Play();
    }

    public void ToggleSilence(){
        silent = !silent;

        if(silent) source.volume = 0;
        else source.volume = 1;

        //lerp volume up and down
    }
}
