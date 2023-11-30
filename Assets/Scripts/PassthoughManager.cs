using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassthoughManager : MonoBehaviour
{
    WebCamDevice[] devices;
    WebCamTexture camTexture;

    public Image bg;

    public Vector2 webcamResolution;

    // Start is called before the first frame update
    void Start()
    {
        devices = WebCamTexture.devices;

        if(devices.Length == 0){
            //Debug.Log("No camera on device");
        }
        else{
            for(int i = 0; i < devices.Length; i++){
                //Debug.Log(devices[i].name);

                if(!devices[i].isFrontFacing){
                }
            }
            camTexture = new WebCamTexture(devices[0].name, (int)webcamResolution.x, (int)webcamResolution.y);

            bg.material.mainTexture = camTexture;

            camTexture.Play();
        }
    }
}
