using System.Collections.Generic;
using UnityEngine;

public class Cameras : MonoBehaviour        //Master class for the app, controls gyro rotation and user input
{
    public enum AccessElements {
        BSL = 0,
        Subtitles = 1,
        AudioDescription = 2
    }

////Public vars////
    [Header("Cam info")]
    public Vector3 rotationFactor;  //Factor applied to gyroscope rotation. can be used to disable rotation on certain axis
    
    [Header("Anchors")]
    public Transform worldAnchor;       //Positional anchor for gameobjects in world space (keep real-world position with head movement)
    public Canvas worldCanvas;          //Canvas for UI elements in world space
    public Transform camAnchor;         //Positional anchor for gameobjects in camera space (move with head movement) 
    public Canvas camCanvas;            //Canvas for UI elements in camera space
    public List<GameObject> worldObjects;   //List of objects (world objects and UI elements) that will be moved between world and camera space when toggling anchor

    [Header("Access Elements")]
    public RectTransform bslVideoRect;      //BSL video element
    public AudioSource audioDescSource;     //Audio player element for audio descriptions. Will need to be developed to a separate class to handle sequencing of multiple audio tracks
    public GameObject subtitlesObject;      //Subtitles

    public AccessElements currentAccessElement = AccessElements.BSL;        //Currently selected access element. All others will be disabled.
    [Range(0,5)] public float touchHoldTime = 1.5f;                         //time for a touch input to be recognised as a hold input in seconds

////Private vars////
    bool isAnchored = true;         // determines if the worldObjects are anchored to the world or the camera
    bool isGyroAvailable = false;   // used to check if the current device has a gyroscope available
    bool holdTimerActive = false;
    bool bslMovingActive = true;
    bool toggleElementsActive = false;

    float holdTimer = 0;    //Timer for tap and hold input
    int currentBSLPosition = 0;
    float[] bslPositions = {-50, -0, 50};

////////////////////
    void Start(){

        if(SystemInfo.supportsGyroscope){
            isGyroAvailable = true;
            Input.gyro.enabled = true;
        }
        else Debug.Log("No gyroscope available on this device.------------------");

        ToggleAnchor();     //Start anchored
        SetAccessElement(currentAccessElement);
    }

    void Update()
    {
        Vector3 rotVec = Input.gyro.rotationRateUnbiased;
        //Debug.Log("RotRateUnbiased x: " + rotVec.x.ToString("0.0") + " y: " + rotVec.y.ToString("0.0") + " z: " + rotVec.z.ToString("0.0"));

        if(Input.touchCount > 0){       //Detect touch
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began){
                // when the touch begins, reset and start touch hold timer
                holdTimer = 0;
                holdTimerActive = true;
            }
            if(holdTimerActive){    //While there is a touch detected, increment the touch hold timer
                holdTimer += Time.deltaTime;
            }

            if(touch.phase == TouchPhase.Ended){    //When the touch ends, evaluate if it was a tap or a hold
                holdTimerActive = false;            //Stop the touch hold timer

                if(holdTimer >= touchHoldTime ){    // hold input detected
                    Debug.Log("Tap and hold---------------");
                    bslMovingActive = !bslMovingActive;     //Toggle the ability to change bsl interpreter position
                    toggleElementsActive = !toggleElementsActive;
                    Debug.Log("BSL movement: " + bslMovingActive + " | access elements: " + toggleElementsActive);
                }
                else{                               // tap input detected
                    Debug.Log("Tap-------------------");
                    
                    if(bslMovingActive){    //Move the bsl video from left>middle>right

                        currentBSLPosition++;   //Increment current position id
                        if(currentBSLPosition >= bslPositions.Length) currentBSLPosition = 0;     //If id > list length, reset id to 0
                        bslVideoRect.anchoredPosition = new Vector2(bslPositions[currentBSLPosition], bslVideoRect.anchoredPosition.y); //Assign bsl video position using array of positions
                    }
                    else if(toggleElementsActive){  //Toggle the bsl, video, and audio on and off

                        int nextEnumInt = (int)currentAccessElement + 1;                //Get the int value of the next enum in the list
                        
                        if(nextEnumInt > 2)currentAccessElement = (AccessElements)0;    //If it is out of range (> 2) then reset to 0 (bsl)
                        else currentAccessElement = (AccessElements)nextEnumInt;        //If it isnt out of range, update the current access element variable

                        Debug.Log("Current access element changed to: " + currentAccessElement);
                        SetAccessElement(currentAccessElement);     //Set the current access element
                    }
                }
            }
        }

        if(isGyroAvailable) {   //If the device has a gyroscope availabe, rotate the cameras and all attatched objects and UI elements accordingly.
            //gyro.rotationRateUnbiased returns the speed of rotation around the axis in radians, processed to remove biases and return a more accurate value
            //gyro.rotationRate returns the raw value from the gyroscope

            transform.Rotate(Input.gyro.rotationRateUnbiased.x * rotationFactor.x, Input.gyro.rotationRateUnbiased.y * rotationFactor.y, Input.gyro.rotationRateUnbiased.z * rotationFactor.z);
        }
    }

    private static Quaternion GyroToUnity(Quaternion q){    // raw gyroscope rotation uses a different handed-ness to Unity, this converts it.
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    private void ToggleAnchor(){    //Toggles the world objects to be anchored to the camera or the world.
        isAnchored = !isAnchored;   //Flip the bool

        foreach(GameObject obj in worldObjects){    //For every world object
            if(isAnchored){                         //Set parent to world

                if(obj.GetComponent<RectTransform>() != null){    //Canvas elements
                    obj.transform.SetParent(worldCanvas.gameObject.transform);  //Put the UI element onto the world canvas
                    Debug.Log(obj.name + " anchored to world canvas.");
                }
                else{
                    obj.transform.SetParent(worldAnchor);           //World objects. Put the world object on the world anchor
                    Debug.Log(obj.name + " anchored to world.");
                }
            }
            else{                                   //Set parent to camera
                if(obj.GetComponent<RectTransform>() != null){    //Canvas elements
                    obj.transform.SetParent(camCanvas.gameObject.transform);    //Put the UI element onto the camera canvas
                    //obj.transform.SetAsFirstSibling();
                    Debug.Log(obj.name + " anchored to camera canvas.");
                }
                else{
                    obj.transform.SetParent(camAnchor);             //World objects. Put the world object on the camera anchor
                    Debug.Log(obj.name + " anchored to camera.");
                }
            }
        }
    }

    private void SetAccessElement(AccessElements el){       //Enables one access element at a time and disables the rest

        switch(el){
            case AccessElements.BSL:
                bslVideoRect.gameObject.SetActive(true);    //Set the BSL object to active
                bslVideoRect.gameObject.GetComponent<UnityEngine.Video.VideoPlayer>().Play();   //Play the BSL video

                audioDescSource.Stop();                     //Stop the subtitles and audio elements and disable them
                //subtitlesObject.gameObject.GetComponent<SubtitleLoader>().StopSubtitles();
                audioDescSource.gameObject.SetActive(false);    
                subtitlesObject.SetActive(false);
                break;
            case AccessElements.Subtitles:
                subtitlesObject.SetActive(true);            //Set the subtitles to active
                //subtitlesObject.gameObject.GetComponent<SubtitleLoader>().StartSubtitles(); //Start the subtitles. Minor bug, first time around the sequence isn't ready to be started.

                bslVideoRect.gameObject.GetComponent<UnityEngine.Video.VideoPlayer>().Stop();   //Stop the video and audio elements and disable them
                audioDescSource.Stop();
                bslVideoRect.gameObject.SetActive(false);
                audioDescSource.gameObject.SetActive(false);
                break;
            case AccessElements.AudioDescription:
                audioDescSource.gameObject.SetActive(true);     //Set the audio object to active
                audioDescSource.Play();                         //Play the audio

                bslVideoRect.gameObject.GetComponent<UnityEngine.Video.VideoPlayer>().Stop();   //Stop the video and subtitles elements and disable them
                //subtitlesObject.GetComponent<SubtitleLoader>().StopSubtitles();
                bslVideoRect.gameObject.SetActive(false);
                subtitlesObject.SetActive(false);
                break;
            default:
                break;
        }

    }
}
