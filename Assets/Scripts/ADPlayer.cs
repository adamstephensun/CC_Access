using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADPlayer : MonoBehaviour
{
    [Header("Refs")]
    public UIElementManager uiElementManager;

    [Header("Clip lists")]
    public AudioClip scene8Audio;       // Replacement clip for scene 8 AD as it is different to all other scenes
    public List<AudioClip> adList = new List<AudioClip>();
    public List<AudioClip> questionExplain = new List<AudioClip>();
    public List<AudioClip> questionADExplain = new List<AudioClip>();

    string debugPrefix = "AD --- ";

    int currentClip;
    bool playingTest;

    public AudioSource source;
    [HideInInspector] public bool adEnabled = true;

    void Start()
    {
        //source = GetComponent<AudioSource>();
    }

    void Update(){
        if(playingTest){
            if(!source.isPlaying && currentClip < adList.Count){
                currentClip++;
                PlayClip(adList[currentClip]);
            }
        }
    }

    public bool PlayADLine(int index){
        
        if(adList.Count > index){                   // If index is within range
            if(index == 7){                             // If index is 7 (play unique audio for when ad is disabled)
                // Play unique scene 8 audio (AD disabled)
                float time = PlayClip(scene8Audio);
                Debug.Log(debugPrefix + "Playing unique scene 8 audio.");
                StartCoroutine(DisableADAfterTime(time));       // Enable AD for *time* seconds (length of clip) then disable
            }
            else{                           // If index != 7 and AD is enabled, play clip
                Debug.Log(debugPrefix + "Playing AD clip not 7, index:" + index);
                PlayClip(adList[index]);
                if(!uiElementManager.adVisibility){
                    SetADActive(false);
                }
            }
            return true;
        }
        return false;
    }

    float PlayClip(AudioClip clip){
        if(clip != null){
            source.clip = clip;
            source.Play();
            Debug.Log(debugPrefix + "Playing clip " + clip.name);
            return clip.length;
        }
        else return 0;
    }

    public void TestPlayAll(){
        currentClip = 0;

        PlayClip(adList[currentClip]);
        playingTest = true;
    }

    public float PlayQuestionLine(int index){
        if(uiElementManager.adVisibility){
            if(questionADExplain.Count > index) PlayClip(questionADExplain[index]);
            else Debug.Log(debugPrefix + "No clip at index " + index + " in questionADExplain list.");
        }
        else{
            if(questionExplain.Count > index){
                float time = PlayClip(questionExplain[index]);
                StartCoroutine(DisableADAfterTime(time));
            } 
            else Debug.Log(debugPrefix + "No clip at index " + index + " in questionExplain list.");
        }

        float length = questionADExplain[index].length;   // Set length to questionAD clip length, AD clip will always be longer than regular.

        return length;
    }

    public void SetADActive(bool b){
        //if(source == null) source = GetComponent<AudioSource>();
        Debug.Log("AD set to: " + b.ToString());
        if(b) source.volume = 1;
        else source.volume = 0;
    }

    public void StopClip(){
        if(source.isPlaying) source.Stop();
    }

    public void PauseClip(){
        if(source.isPlaying) source.Pause();
    }

    public void UnPauseClip(){
        if(!source.isPlaying) source.UnPause();
    }

    IEnumerator DisableADAfterTime(float time){
        Debug.Log("Disable AD after time for " + time + " seconds");
        SetADActive(true);
        yield return new WaitForSeconds(time);
        SetADActive(false);
    }
}
