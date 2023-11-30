using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SubtitlePlayer : MonoBehaviour
{
    [Header("UI objects")]
    public TextMeshProUGUI subTextUI;

    [Header("Subtitles")]
    //public bool ignoreStartTime;        // If true, the first line of the subgroup plays immediately. If false, it plays with the timing in the json file
    public SubGroup currentGroup;
    [HideInInspector] public List<SubGroup> subList = new List<SubGroup>();

    // Private vars //

    int numOfLines = 0;
    int currentLine = 0;
    int ind;
    float prevTime;

    string debugPrefix = "SUB PLAYER --- ";

    bool subsLoaded = false;
    bool coRunning = false;
    bool stopCo = false;
    bool restartCoOnInterrupt = true;

    //DelayManager.Sequence seq;
    //List<int> indexes = new List<int>();        // lists of indexs and timings for the sequence
    //List<float> timings = new List<float>();


    /*public void LoadSubFile(){
        string jsonString = subFile.text;
        subs = JsonUtility.FromJson<Subtitles>(jsonString);
        subsLoaded = true;

        numOfLines = subs.lines.Count;

        Debug.Log("Subtitle file loaded");
    }

    public void ShowLine(int _index){
        // shows a single subtitle line at _index

        if(_index < 0 || _index > subs.lines.Count){
            Debug.Log("Index " + _index + " is out of range");
        }
        else{
            subTextUI.text = subs.lines[_index];
            Debug.Log("Showing line index: " + _index);
        }
    }

    public void ShowLineGroup(int _start, int _number, int _timing = 6){
        // shows _number amount of subtitle lines starting at _start, each playing for _timing seconds
        
        int start = _start;
        int number = _number;
        int timing = _timing;
        int end = start + number;

        Debug.Log("Showing " + number + " lines starting at " + start + " and ending at " + end +" at " + timing + " seconds each.");
        
        seq = new DelayManager.Sequence();
        List<int> indexes = new List<int>();

        for(int i = start; i < end; i++){
            indexes.Add(i);
        }

        foreach(int i in indexes){      // have to split into for loop and foreach loop because of DelayManager bug
            seq.Add(x => ShowLine(i));
            seq.Delay(timing);
        }

        seq.Add(x => GroupFinished());

        seq.Invoke();
    }*/

    void Update(){
        if(stopCo){
            if(!coRunning){
                Debug.Log(debugPrefix + "COROUTINE INTERRUPTED");
                stopCo = false;
                subTextUI.text = "";
                currentGroup = subList[ind];

                currentLine = 0;

                if(restartCoOnInterrupt) StartCoroutine(subCoroutine());
            }
        }
    }

    public void ShowLine(int index){
        if(currentGroup.captions[index] != null){
            subTextUI.text = currentGroup.captions[index].content;
        }
        else{
            Debug.LogWarning(debugPrefix + "Trying to play sub line " + index + ". Out of range.");
        }
    }

    public bool PlayGroupNew(int index){
        if(subsLoaded){
            if(subList[index] != null){
                
                if(coRunning){
                    stopCo = true;
                    restartCoOnInterrupt = true;
                    ind = index;
                }
                else{
                    subTextUI.text = "";
                    currentGroup = subList[index];

                    //isFirstLine = true;
                    currentLine = 0;

                    StartCoroutine(subCoroutine());
                }

                return true;
            }
            else{
                Debug.Log("SUBS --- SubList at index " + index + " is null and cannot be loaded.");
                return false;
            }
        }
        else{
            Debug.Log("SUBS --- Cannot play sub group " + index + ", no subs loaded");
            return false;
        }
    }

    IEnumerator subCoroutine(){
        coRunning = true;
        // Set initial delay for first line
        if(currentLine == 0){
            float startDelay = (float)TimeSpan.Parse(currentGroup.captions[currentLine].startTime).TotalSeconds;
            Debug.Log(debugPrefix + "First line. Waiting for *" + startDelay + "* seconds then displaying line: " + currentLine);
            yield return new WaitForSeconds(startDelay);
            prevTime = startDelay;
        }

        // Set subtitle text to current line content
        subTextUI.text = currentGroup.captions[currentLine].content;
        //Debug.Log("SUBS --- Displaying line " + currentLine + " in subs file " + currentGroup.name);

        // Check if this is the last line. If so, end. If not, iterate current line and loop coroutine.
        if(currentLine == currentGroup.captions.Count - 1){
            yield return new WaitForSeconds(3);
            GroupFinished();
            coRunning = false;
            yield break;
        }
        else{
            // Calculate time difference and wait
            float nextLineStartTime = (float)TimeSpan.Parse(currentGroup.captions[currentLine + 1].startTime).TotalSeconds;
            float delay = nextLineStartTime - prevTime;
            prevTime = nextLineStartTime;

            //Debug.Log("SUBS --- Waiting for *" + delay + "* seconds before showing next line at index " + (currentLine + 1) + " then looping coroutine.");
            yield return new WaitForSeconds(delay);
            
            // Iterate current line and loop coroutine
            currentLine++;
            if(!stopCo) StartCoroutine(subCoroutine());
            else{
                coRunning = false;
                yield break;
            }
        } 
    }

    /*public bool PlayGroup(int index){
        if(subsLoaded){
            if(subList[index] != null){
                ClearSubs();
                currentGroup = subList[index];
                Debug.Log("Playing sub group " + index + "  name: " + currentGroup.name + ". Ignore start time: " + ignoreStartTime.ToString());

                string startTime = currentGroup.captions[0].startTime;
                float initialDelay = (float)TimeSpan.Parse(startTime).TotalSeconds;

                float prevTime = initialDelay;
                //DelayManager.instance.CancelInvoke();
                seq = new DelayManager.Sequence();
                if(!ignoreStartTime) seq.InitialDelay(initialDelay);

                indexes.Clear();
                timings.Clear();

                if(ignoreStartTime) indexes.Add(0);

                if(ignoreStartTime) prevTime = (float)TimeSpan.Parse(currentGroup.captions[0].startTime).TotalSeconds;

                int startIndex = 0;
                if(ignoreStartTime) startIndex = 1;

                for(int i = startIndex; i < currentGroup.captions.Count; i++){

                    float _startTime = (float)TimeSpan.Parse(currentGroup.captions[i].startTime).TotalSeconds;
                    float delay = _startTime - prevTime;
                    prevTime = _startTime;

                    indexes.Add(i);
                    timings.Add(delay);
                }

                if(ignoreStartTime) timings.Add(4);

                foreach(int i in indexes){
                    seq.Add(x => ShowLine(i));
                    seq.Delay(timings[i]);
                }

                seq.Add(x => GroupFinished());
                seq.Invoke();
                return true;
            }
            else{
                Debug.Log("SubList at index " + index + " is null and cannot be loaded.");
                return false;
            }
            
        }
        else{
            Debug.Log("Cannot play sub group " + index + ", no subs loaded");
            return false;
        }
        
    }*/

    public void SubsLoaded(List<SubGroup> _subList){
        subList = _subList;
        subsLoaded = true;
        numOfLines = subList.Count;

        //indexes.Clear();
        //timings.Clear();
    }

    void GroupFinished(){
        Debug.Log(debugPrefix + "Sub group finished at line: " + currentLine);
        subTextUI.text = "";
    }

    public void ClearSubs(){
        DelayManager.instance.CancelInvoke();
        subTextUI.text = "";
        currentGroup = null;
        //seq = null;
    }

    public void StopSubs(){
        if(coRunning){
            stopCo = true;
            restartCoOnInterrupt = false;
        }
    }
 
    public int GetNumOfLines(){
        return numOfLines;
    }

    public void PauseSubs(){
        
    }

    public void UnPauseSubs(){

    }
}
