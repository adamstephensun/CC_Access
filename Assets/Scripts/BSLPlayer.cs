using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BSLPlayer : MonoBehaviour
{

    public VideoPlayer videoPlayer;
    public VideoClip currentClip;
    public UIElementManager uiManager;

    public List<VideoClip> videos = new List<VideoClip>();

    bool videoPlayed = false;
    string debugPrefix = "BSL --- ";

    Renderer videoRenderer;

    void Start()
    {
        videoPlayer.playOnAwake = false;

        videoRenderer = videoPlayer.gameObject.GetComponent<Renderer>();

        if(videoPlayed && !videoPlayer.isPlaying){
            Debug.Log(debugPrefix + "Video finished: " + videoPlayer.clip.name);
            videoPlayed = false;
            videoRenderer.material.SetInt("_Visible", 0);
        }
    }

    public bool PlayVideo(int index){
        if(videos.Count > index){
        
            videoPlayer.Stop();
            videoPlayer.clip = videos[index];
            currentClip = videoPlayer.clip;
            videoPlayer.Play();
            Debug.Log(debugPrefix + "Playing video clip: " + videoPlayer.clip.name +" at index " + index);


            if(videoRenderer.material.GetInt("_Visible") == 0 && uiManager.bslVisibility){
                videoRenderer.material.SetInt("_Visible", 1);
            }

            videoPlayed = true;
            return true;
        }
        else{
            Debug.LogWarning(debugPrefix + "Trying to play video " + index + ". Out of bounds.");
            return false;
        }

    }

    public void StopBSL(){
        if(videoPlayer.isPlaying) videoPlayer.Stop();
    }

    public void PauseBSL(){
        if(videoPlayer.isPlaying) videoPlayer.Pause();
    }

    public void UnPauseBSL(){
        if(!videoPlayer.isPlaying) videoPlayer.Play();
    }
}
