using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using TMPro;

public class MasterSimul : MonoBehaviour
{
    public string address = "/ar_access/address";
    public string connectionAddress = "/ar_access/connection";
    public string triggerAddress = "/ar_access/trigger";

    public List<string> phoneIPs = new List<string>();    

    public TextMeshProUGUI debugText;

    bool connectionConfirmed = false;
    int connectionsConfirmed = 0;

    OSCReceiver reciever;
    OSCTransmitter transmitter;

    void Start()
    {
        reciever = GetComponent<OSCReceiver>();
        reciever.Bind(connectionAddress, ReceivedConnectionMessage);

        transmitter = GetComponent<OSCTransmitter>();
    }

    public void SendTestMessages(){
        var message = new OSCMessage("/questions/1");
        for(int i = 1; i < 21 ; i++){
            transmitter.RemoteHost = "192.168.2." + i;
            transmitter.Send(message);
            Debug.Log("Sent message: " + message.Address + " to IP: " + transmitter.RemoteHost);
        }   
    }

    public void SendConnectionMessages(){
        var message = new OSCMessage(connectionAddress);

        message.AddValue(OSCValue.Int(1));

        for(int i = 0 ; i < phoneIPs.Count; i++){
            transmitter.RemoteHost = phoneIPs[i];
            transmitter.Send(message);
            Debug.Log("Connection message send to device number " + i + " with IP: " + phoneIPs[i]);
        }
        Debug.Log("All connection messages sent");
    }

    public void SendTriggerMessages(int m){
        var message = new OSCMessage(triggerAddress);
        message.AddValue(OSCValue.Int(m));

        for(int i = 0 ; i < phoneIPs.Count; i++){
            transmitter.RemoteHost = phoneIPs[i];
            transmitter.Send(message);
            Debug.Log("Trigger: " + m + " sent to device number " + i + " with IP: " + phoneIPs[i]);
        }
    }

    public void SendTestMessage(){
        var message = new OSCMessage(address);

        if(Random.value > 0.5) message.AddValue(OSCValue.String("hello osc world"));
        else message.AddValue(OSCValue.Int(Random.Range(0,10)));

        transmitter.Send(message);
    }

    void ReceivedConnectionMessage(OSCMessage message){

        Debug.LogFormat("NETWORK R - Received connection message: " + message);

        connectionsConfirmed++;
        debugText.text = connectionsConfirmed + " devices confirmed.";

        if(connectionsConfirmed == phoneIPs.Count){
            Debug.Log("All connections confirmed.");
            connectionConfirmed = true;
            debugText.text = "All devices confirmed (" + connectionsConfirmed + ").";
        }
    }
}
