using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using extOSC;
using TMPro;
using System;

public class Transmitter : MonoBehaviour
{

    [Header("---SET DEVICE ID---")]
    public int deviceID;

    [Header("Addresses")]
    public string connectionAddress = "/ar_access/connection";
    public string masterIP = "192.168.0.20";

    [Header("Message sending")]
    public int _message;
    public bool sendMessage = false;
    public string inputMessagePrefix = "/eos/chan/";
    public string inputMessageSuffix = "/param/red/green/blue";
    public string fullInputMessageAddress;

    public string macroAddressPrefix = "/eos/macro/";
    public string macroAddressSuffix = "/fire";
    
    [Header("UI elements")]
    public TextMeshProUGUI addressesUI;
    public TMP_InputField masterIPInput;
    public TMP_InputField deviceIDInput;

    // GameObject references
    OSCTransmitter transmitter;
    NetworkManager netManager;

    string ownIP;
    string startingMasterIP;   // save initial master IP address so it can be reset in UI
    int startingDeviceID;

    void Start()
    {
        netManager = FindObjectOfType<NetworkManager>();
        transmitter = GetComponent<OSCTransmitter>();
        transmitter.RemoteHost = masterIP;

        GetLocalIPv4();

        fullInputMessageAddress = inputMessagePrefix + deviceID + inputMessageSuffix;

        masterIPInput.text = masterIP;
        startingMasterIP = masterIP;
        startingDeviceID = deviceID;

        deviceIDInput.text = deviceID.ToString();
    }

    public void SendConnectionMessage(){

        var connectionMessage = new OSCMessage(connectionAddress);
        connectionMessage.AddValue(OSCValue.Int(2));

        transmitter.Send(connectionMessage);
        Debug.Log("NETWORK T - Connection message sent. Address: " + connectionAddress + ", message: " + 2);

    }

    public void SendInputMessage(Color col, float delay){
        // sends input message with colour and device num to  lighting desk
        // currently sends raw rgb value from 0-1, may need to change to fit lighting desk input
        // delay of 5 for regular question, 20 for orb on 

        int r, g, b;

        r = (int)(col.r * 100.0f);
        g = (int)(col.g * 100.0f);
        b = (int)(col.b * 100.0f);

        var inputMessage = new OSCMessage(fullInputMessageAddress);
        inputMessage.AddValue(OSCValue.Int(r));
        inputMessage.AddValue(OSCValue.Int(g));
        inputMessage.AddValue(OSCValue.Int(b));

        transmitter.Send(inputMessage);
        
        StartCoroutine(LightMessages(delay));

        //Debug.Log("Sent input message: " + fullInputMessageAddress + " - r" + r + "  g" + g + "  b" + b);
    }

    public void SendRelayMessages(OSCMessage message){
        StartCoroutine(Relay(message));
    }

    IEnumerator Relay(OSCMessage message){
        // Coroutine to send relay messages twice to avoid dropped packages
        transmitter.RemotePort = 8001;
        
        foreach(int id in netManager.relayIDs){
            transmitter.RemoteHost = "192.168.2." + id.ToString();
            transmitter.Send(message);
            Debug.Log("Sent relay message to device ID: " + id + "  message: " + message);
        }

        yield return new WaitForSeconds(0.5f);

        foreach(int id in netManager.relayIDs){
            transmitter.RemoteHost = "192.168.2." + id.ToString();
            transmitter.Send(message);
            Debug.Log("Sent relay message to device ID: " + id + "  message: " + message);
        }

        transmitter.RemotePort = 8000;
        transmitter.RemoteHost = masterIP;
    }

    public void RelayMessageTest(){
        if(netManager.relayMessages){
            OSCMessage message = new OSCMessage("/questions/2");
            Debug.Log("Sending test relay message: " + message.Address);

            SendRelayMessages(message);
        }
        else{
            Debug.Log("This is not a relay device. Cannot send relay message test");
        }
    }

    public void GetLocalIPv4()
    {
        var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

        string addresses = "";

        //Debug.Log("NETWORK T - PRINTING ADDRESSES");
        if(addressList.Length > 0){
            foreach(var element in addressList){
                //Debug.Log(element);
                addresses += element + "<br>";
            }
        }
        else Debug.Log("NETWORK T - NO ADDRESSES IN LIST");
        //Debug.Log("NETWORK T - FINSIHED PRINTING ADDRESSES");
        Debug.Log(addresses);

        if(Application.platform == RuntimePlatform.WindowsEditor){
            ownIP = addressList[1].ToString();
        }
        else if(Application.platform == RuntimePlatform.Android){
            ownIP = addressList[0].ToString();
        }

        addressesUI.text = addresses;

        int _id = int.Parse(ownIP[10..]);
        deviceID = _id;
        Debug.Log("ID from IP: " + _id);
    }

    public IEnumerator LightMessages(float delay){

        var macro1Message = new OSCMessage(macroAddressPrefix + deviceID.ToString() + macroAddressSuffix);
        var macro2Message = new OSCMessage(macroAddressPrefix + (deviceID + 100).ToString()  + macroAddressSuffix);

        // send message 1
        transmitter.Send(macro1Message);

        Debug.Log("Sent light message 1: " + macro1Message);
        // wait 5 seconds
        yield return new WaitForSeconds(delay);
        // send message 2
        transmitter.Send(macro2Message);
        Debug.Log("Sent light message 2: " + macro2Message);

    }

    public void UpdateMasterIP(){
        masterIP = masterIPInput.text;

        PlayerPrefs.SetString("MasterIP", masterIP);
    }

    public void ResetMasterAddress(){
        masterIPInput.text = startingMasterIP;
        masterIP = startingMasterIP;

        PlayerPrefs.SetString("MasterIP", masterIP);
    }

    public void UpdateDeviceID(){
        deviceID = int.Parse(deviceIDInput.text);
        fullInputMessageAddress = inputMessagePrefix + deviceID + inputMessageSuffix;

        PlayerPrefs.SetInt("ID", deviceID);
    }

    public void ResetDeviceID(){
        deviceIDInput.text = startingDeviceID.ToString();
        deviceID = startingDeviceID;
        fullInputMessageAddress = inputMessagePrefix + deviceID + inputMessageSuffix;

        PlayerPrefs.SetInt("ID", deviceID);
    }


}
