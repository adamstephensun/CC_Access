using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingBackup : MonoBehaviour
{

    public NetworkManager networkManager;

    [Header("Custom timer")]
    public float currentTime;
    public float target;
    public bool moveToNext;
    bool timerActive = false;
    bool timerInterrupt = false;

    [Header("Commands")]
    public int currentCommand = 0;

    public bool startTimerOnStart;
    public bool commandRecieveTest = false;

    [Header("Command list")]
    public List<Command> commands = new List<Command>();

    // Private //////

    string debugPrefix = "BACKUP --- ";
    CoRunner coRunner;

    void Start()
    {
        float totalTime = 0;

        coRunner = GetComponent<CoRunner>();
        foreach(Command c in commands){
            c.coRunner = coRunner;
            c.isTriggered = false;
            totalTime += c.deltaTiming;
        } 

        Debug.Log("Total time: " + totalTime);

        if(startTimerOnStart) StartTimer(commands[currentCommand].deltaTiming);
    }

    void Update()
    {
        if(commandRecieveTest){
            CommandRecieved(new Command(coRunner, CommandType.Subs, 4, 15));
            commandRecieveTest = false;
        }

        if(timerActive){
            currentTime += Time.deltaTime;
            if(currentTime >= target && !timerInterrupt){
                Debug.Log("Timer up on command: " + currentCommand);
                timerActive = false;
                
                if(moveToNext){
                    SendCommandToNetManager(commands[currentCommand]);
                    Debug.Log("Movig to next command from update - if(moveToNext) line 64");
                    NextCommand();
                }
                else{
                    Debug.Log("Not moving to next command in list, reset command was received.");
                }
            }
            else if(timerInterrupt){
                Debug.Log("Timer interrupt");
                timerActive = false;
            }
        }
    }

    void StartTimer(float _target){
        currentTime = 0;
        target = _target;
        timerActive = true;
        timerInterrupt = false;
        moveToNext = true;

        Debug.Log("Timer started for " + target + " seconds. Current command: " + currentCommand);
    }

    public void StopTimer(){
        moveToNext = false;
        timerActive = false;
        currentTime = 0;
    }

    public void CommandRecieved(Command command){

        if(timerActive) timerInterrupt = true;

        int pos = commands.IndexOf(command);
        currentCommand = pos;
        Debug.Log("Command recieved. type: " + command.type + "   index: " + command.index + "   at pos: " + pos + " in command list");

        NextCommand();
    }

    void NextCommand(){
        currentCommand++;

        if(currentCommand < commands.Count){
            StartTimer(commands[currentCommand].deltaTiming);
        }
        else{
            Debug.Log("Commands list finished");
        }
    }

    void SendCommandToNetManager(Command command){
        switch(command.type){
            case CommandType.Default:
                break;
            case CommandType.Subs:
                networkManager.DisplaySub(command, true);
                break;
            case CommandType.Questions:
                networkManager.DisplayQuestion(command, true);
                break;
            case CommandType.GiveLife:
                networkManager.DisplayGiveLife(command, true);
                break;
            case CommandType.EndScreen:
                networkManager.DisplayEndScreen(command, true);
                break;
        }
    }

    public enum CommandType {Default, Subs, Questions, GiveLife, EndScreen};
}

[Serializable]
public class Command{
    public TimingBackup.CommandType type;
    public int index;
    public float deltaTiming;

    public bool isTriggered;

    public CoRunner coRunner;

    public Command(CoRunner _co, TimingBackup.CommandType _type = TimingBackup.CommandType.Default, int _index = 0, float _deltaTiming = 10){
        deltaTiming = _deltaTiming;
        type = _type;
        index = _index;
        coRunner = _co;
    }

    public void Trigger(){
        isTriggered = true;
        coRunner.Run(TriggerTimer());
    }

    IEnumerator TriggerTimer(){
        yield return new WaitForSeconds(2);
        isTriggered = false;
    }
}

