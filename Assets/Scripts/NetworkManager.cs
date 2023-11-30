using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using TMPro;
using System;
using UnityEngine.AI;

public class NetworkManager : MonoBehaviour
{
    // Recieves OSC messages from the master system (Unreal) and sends triggers to subtitle, bsl, and audio description managers
    
    [Header("Addresses")]
    public string resetAddress = "/reset/0";
    public string giveLifeAddress = "/questions/0";
    public string endScreenAddress = "/questions/11";
    
    [Header("Relay vars")]
    public int relayDeviceID = 16;      //ID of device to send relay to others
    public List<int> relayIDs = new List<int>();

    [Header ("Manager references")]
    public SubtitlePlayer subPlayer;
    public BSLPlayer bslPlayer;
    public ADPlayer adPlayer;
    public Transmitter transmitter;
    public UserInputManager inputManager;
    public UIElementManager uIElementManager;
    public TimingBackup timingBackup;

    [Header("Debug")]
    public string debugPrefix = "NETWORK RECIEVER --- ";
    public TextMeshProUGUI debugUI;
    public bool subPlaying = false;
    public bool bslPlaying = false;
    public bool adPlaying = false;
    public bool questionPlaying = false;

    //Private
    public OSCReceiver reciever;

    List<int> subsMessagesRecieved = new List<int>();
    List<int> questionMessagesRecieved = new List<int>();

    int numOfQuestions;
    int numOfSubtitles;

    bool giveLifeMessageRecieved = false;
    bool endScreenMessageRecieved = false;

    [HideInInspector] public bool relayMessages = false;
    
    void Start()
    {
        if(transmitter.deviceID == relayDeviceID){
            relayMessages = true;
            Debug.Log(debugPrefix + "This device " + transmitter.deviceID + " will relay messages");
        } 
        if(relayIDs.Contains(transmitter.deviceID)){
            Debug.Log(debugPrefix + "This device will receive relay messages");
        }
    }

    public void InitListeners(){

        numOfQuestions = inputManager.getNumOfQuestions();
        numOfSubtitles = subPlayer.subList.Count;

        // add listeners for the number of subtitle triggers
        if(numOfSubtitles > 0){         
            string debugString = debugPrefix + "Bound subs address: ";
            for(int i = 1 ; i <= numOfSubtitles; i++){
                string str = "/subs/" + i.ToString();
                reciever.Bind(str, RecievedSubsMessage);

                debugString += str + " | ";
            }
            //Debug.Log(debugString);
        }

        // add listeners for the number of question triggers
        if(numOfQuestions > 0){         
            string debugString = debugPrefix + "Bound questions address: ";
            for(int i = 1 ; i <= numOfQuestions; i++){
                string str = "/questions/" + i.ToString();
                reciever.Bind(str, RecievedQuestionsMessage);
                
                debugString += str + " | ";
            }
            //Debug.Log(debugString);
        }

        reciever.Bind(giveLifeAddress, RecievedGiveLifeMessage);
        reciever.Bind(resetAddress, RecievedResetMessage);
        reciever.Bind(endScreenAddress, RecievedEndScreenMessage);
    }

    void RecievedSubsMessage(OSCMessage message){
        string address = message.Address;
        int subInt = int.Parse(address[6..]);

        Debug.Log(debugPrefix + "Recieved subs message address: " + address + "  extracted int: " + subInt);

        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.Subs && c.index == subInt){
                DisplaySub(c, false);
                break;
            }
        }
        
        if(relayMessages){
            transmitter.SendRelayMessages(message);
        }
    }

    public void DisplaySub(Command c, bool fromTimer){

        if(!c.isTriggered){    
            subPlaying = subPlayer.PlayGroupNew(c.index - 1);
            bslPlaying = bslPlayer.PlayVideo(c.index - 1);
            adPlaying = adPlayer.PlayADLine(c.index - 1);
            if(!fromTimer){
                timingBackup.CommandRecieved(c);
                Debug.Log("Display sub called not from timer, sending commandReceived to timing backup");
            }
            else{
                Debug.Log("Display sub called from timer, not sending commandReceived to timing backup");
            }
            c.Trigger();
        }
        else{
            Debug.Log(debugPrefix + "Ignoring double subs message received. index: " + c.index);
        }
    }

    void RecievedQuestionsMessage(OSCMessage message){
        // Receive message to display user input 
        string address = message.Address;
        int questionInt = int.Parse(address[11..]);
        
        Debug.Log(debugPrefix + "Recieved question message address: " + address + "  extracted int: " + questionInt);

        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.Questions && c.index == questionInt){
                DisplayQuestion(c, false);

                break;
            }
        }

        if(relayMessages){
            transmitter.SendRelayMessages(message);
        }
    }

    public void DisplayQuestion(Command c, bool fromTimer){
        if(!c.isTriggered){
            inputManager.ShowInputPrompt(c.index - 1);     
            c.Trigger();
            if(!fromTimer){
                timingBackup.CommandRecieved(c);
                Debug.Log("Display question called not from timer, sending commandReceived to timing backup");
            }
            else{
                Debug.Log("Display question called from timer, not sending commandReceived to timing backup");
            }
        }
        else{
            Debug.Log(debugPrefix + "Double question message received. Ignoring");
        }
    }

    void RecievedResetMessage(OSCMessage message){
        //Reset
        Debug.Log(debugPrefix + "Recieved reset message from master.");

        Reset();

        if(relayMessages){
            transmitter.SendRelayMessages(message);
        }
    }

    void RecievedGiveLifeMessage(OSCMessage message){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.GiveLife){
                //DisplayGiveLife(c, false);
                break;
            }
        }

        if(relayMessages) transmitter.SendRelayMessages(message);
    }

    void RecievedEndScreenMessage(OSCMessage message){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.EndScreen){
                DisplayEndScreen(c, false);
                
                break;
            }
        }

        if(relayMessages) transmitter.SendRelayMessages(message);
    }

    public void DisplayGiveLife(Command c, bool fromTimer){
        if(!c.isTriggered){
            inputManager.ShowGiveLife();
            if(!fromTimer){
                timingBackup.CommandRecieved(c);
                Debug.Log("Display give life called not from timer, sending commandReceived to timing backup");
            }
            else{
                Debug.Log("Display give life called from timer, not sending commandReceived to timing backup");
            }
            c.Trigger();
        }
        else{
            Debug.Log(debugPrefix + "Give life message (/questions/0) already recieved once.");
        }
    }

    public void DisplayEndScreen(Command c, bool fromTimer){
        if(!c.isTriggered){
            uIElementManager.OpenEndScreen();
            if(!fromTimer){
                timingBackup.CommandRecieved(c);
                Debug.Log("Display end screen called not from timer, sending commandReceived to timing backup");
            }
            else{
                Debug.Log("Display end screen called from timer, not sending commandReceived to timing backup");
            }
            c.Trigger();
        }
        else{
            Debug.Log(debugPrefix + "End screen message (/questions/11) already recieved once.");
        }
    }

    public void TestEndScreen(){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.EndScreen){
                DisplayEndScreen(c, false);
            }
        }
    }

    public void TestGiveLife(){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.GiveLife){
                DisplayGiveLife(c, false);
            }
        }
    }

    public void TestLine(int index){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.Subs && c.index == index){
                DisplaySub(c, false);
            }
        }
    }

    public void TestQuestion(int index){
        foreach(Command c in timingBackup.commands){
            if(c.type == TimingBackup.CommandType.Questions && c.index == index){
                DisplayQuestion(c, false);
            }
        }
    }

    public void Reset(){

        subPlayer.StopSubs();
        bslPlayer.StopBSL();
        adPlayer.StopClip();

        inputManager.ClearInputPrompt();
        timingBackup.StopTimer();
    }
}
