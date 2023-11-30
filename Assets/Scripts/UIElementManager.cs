using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UIElementManager : MonoBehaviour
{
    // Manages the UI elements on screen, toggling subs/bsl/audio and scaling

    [Header("References")]
    public GameObject bg;
    public GameObject captionsParent;
    public GameObject debugMenu;
    public GameObject endScreen;

    public TextMeshProUGUI subtitles;
    public GameObject bslVideo;
    public ADPlayer adPlayer;

    public Slider subScaleSlider;

    [Header("Textures")]
    public Texture audioOnTexture;
    public Texture audioOffTexture;
    public Texture captionsOnTexture;
    public Texture captionsOffTexture;
    public Texture bslOnTexture;
    public Texture bslOffTexture;
    public Texture cameraOnTexture;
    public Texture cameraOffTexture;

    [Header("Button image references")]
    public RawImage audioImage;
    public RawImage captionsImage;
    public RawImage bslImage;
    public RawImage cameraImage;

    [Header("Vars")]
    public Vector2 bgFitSize;
    public Vector2 bgStretchSize;

    public bool adVisibility = false;
    public bool capsVisibility = false;
    public bool bslVisibility = false;
    public bool passthroughVisibility = false;

    bool bgStretch = true;
    bool debugMenuHeld = false;

    float debugMenuTimer = 0;
    float holdTime = 2;

    void Start(){

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if(bgStretch) bg.GetComponent<RectTransform>().sizeDelta = bgStretchSize;
        else bg.GetComponent<RectTransform>().sizeDelta = bgFitSize;

        audioImage.texture = audioOnTexture;
        captionsImage.texture = captionsOnTexture;
        bslImage.texture = bslOnTexture;
        cameraImage.texture = cameraOffTexture;

        debugMenu.SetActive(false);
        endScreen.SetActive(false);

        SetADVisibility(adVisibility);
        SetCapsVisibility(capsVisibility);
        SetBSLVisibility(bslVisibility);
        SetPassthroughVisibility(passthroughVisibility);
    }

    void Update(){
        if(debugMenuHeld){
            debugMenuTimer += Time.deltaTime;
            if(debugMenuTimer >= holdTime){
                debugMenu.SetActive(true);
            }
        }
    }

    public void ToggleBGStretch(){
        bgStretch = !bgStretch;

        if(bgStretch) bg.GetComponent<RectTransform>().sizeDelta = bgStretchSize;
        else bg.GetComponent<RectTransform>().sizeDelta = bgFitSize;
    }

    public void ToggleADVisibility(){
        SetADVisibility(!adVisibility);
    }

    public void ToggleCapsVisibility(){
        SetCapsVisibility(!capsVisibility);
    }

    public void ToggleBSLVisibility(){
        SetBSLVisibility(!bslVisibility);
    }

    public void TogglePassthroughVisibility(){
        SetPassthroughVisibility(!passthroughVisibility);
    }

    void SetADVisibility(bool b){
        adVisibility = b;

        adPlayer.SetADActive(adVisibility);

        if(adVisibility){
            audioImage.texture = audioOnTexture;
            audioImage.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        } 
        else{
            audioImage.texture = audioOffTexture;
            audioImage.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);    //ad_off texture is flipped, reverse x scale
        } 
    }

    void SetCapsVisibility(bool b){
        capsVisibility = b;

        captionsParent.SetActive(capsVisibility);

        if(capsVisibility){
            captionsImage.texture = captionsOnTexture;
            SetPassthroughVisibility(true);
        } 
        else captionsImage.texture = captionsOffTexture;
    }

    void SetBSLVisibility(bool b){
        bslVisibility = b;

        if(bslVisibility){
            bslVideo.GetComponent<Renderer>().material.SetInt("_Visible", 1);     // Set chroma key material visibility parameter
            SetPassthroughVisibility(true);     // Enable passthrough when BSL is enabled
            bslImage.texture = bslOnTexture;
        } 
        else{
            bslVideo.GetComponent<Renderer>().material.SetInt("_Visible", 0);
            bslImage.texture = bslOffTexture;
        } 
    }

    void SetPassthroughVisibility(bool b){
        passthroughVisibility = b;

        if(passthroughVisibility){
            cameraImage.texture = cameraOnTexture;
            bg.GetComponent<Image>().color = Color.white;
        } 
        else{
            cameraImage.texture = cameraOffTexture;
            bg.GetComponent<Image>().color = Color.black;
        } 
    }

    public void ScaleSubs(){
        float newScale = subScaleSlider.value;
        captionsParent.transform.localScale = new Vector3(newScale, newScale, newScale);
        Debug.Log("Setting scale to: " + newScale);
    }

    public void DebugMenuDown(){
        debugMenuHeld = true;
        debugMenuTimer = 0;
    }

    public void DebugMenuUp(){
        debugMenuHeld = false;
    }

    public void OpenEndScreen(){
        endScreen.SetActive(true);
        endScreen.GetComponent<EndScreen>().InitialiseEndScreen();
    }

    public void CloseEndScreen(){
        endScreen.SetActive(false);
    }
}
