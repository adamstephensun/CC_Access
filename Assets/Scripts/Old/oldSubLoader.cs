/*public class OldSubtitleLoader : MonoBehaviour     //This script handles loading and displaying subtitle files. Is attached to a TMP text object.
{

    /*
        Subtitle files are loaded as JSON in a certain format. Open one of the files in the Resources folder for an example.
        A subtitle file consists of metadata ("name", "usingDuration") and a list of lines. Each line contains "text" and "time".
        The "usingDuration" bool determines if the "time" variable is interpreted as the duration of each line (true) or the end time of each line from the start (false).
    */
/*
    public TextMeshProUGUI tmpText;     //UI element that displays the text

    public Subtitles subs;                      //Object containing the subtitle information from the JSON file (text, time, metadata)
    public TextAsset subtitlesFile;             //The JSON file containing the subtitles
    public DelayManager.Sequence sequence;      //The sequence that is populated with subtitle text and delay times

    [Range(0,10)] public float defaultSubTiming = 5;    //Default timing used to override timing in JSON. If == 0, use JSON timings
    public bool loop;                           //When true, subtitles loop to beginning when finished.

    void Start()
    {
        loadSubtitlesFile();
        InitSubtitles();
        //StartSubtitles();
    }

    void InitSubtitles(){       //Creates the subtitle sequence using the data loaded from the JSON

        Debug.Log("Subtitle file: " + subs.name + "  usingDuration: " + subs.usingDuration + "  with " + subs.lines.Count + " lines loaded.");

        sequence = new DelayManager.Sequence();     //Clear the sequence

        if(subs.usingDuration){     //If the subs file is using duration, the "time" variables dictates the amount of time for each line to be displayed
            foreach(var line in subs.lines){
                sequence.Add(x => UpdateTMP(line.text));                    //For each line in the JSON file, add the text to the sequence
                if(defaultSubTiming > 0) sequence.Delay(defaultSubTiming);  //Add the delay time for each line to the sequence. Checks if defaultSubTiming is used as an override
                else sequence.Delay(line.time);
            }
        }
        else{   //If the subs file is not using duration, each line has its end time in seconds which is used the calculate the duration of each line
            for(int i = 0; i < subs.lines.Count ; i++){
                float duration;
                if(i == 0) duration = subs.lines[i].time;   //If this is the first line, use its end time as the duration.
                else duration = subs.lines[i].time - subs.lines[i-1].time;  //For subsequent lines, calculate the duration by subtracting the previous end time from the current end time

                if(duration <= 0) duration = 3;     //If the calcualted duration is less than or equal to 0 (mistake in writing JSON file), set to a default value of 3.
                
                string text = subs.lines[i].text;   //Get the text from the current line
                
                Debug.Log("Adding line: " + text + " | with duration: " + duration);

                sequence.Add(x => UpdateTMP(text));                         //Add the text and delay to the subtitle sequence, checking if defaultSubTiming is used as an override
                if(defaultSubTiming > 0) sequence.Delay(defaultSubTiming);
                else sequence.Delay(duration);
            }
        }

        sequence.Add(x => SubtitlesFinished());     //Tell the program that the sequence has ended at the end
    }

    public void StartSubtitles(){       //Starts the sequence that was initialised in InitSubtitles()
        sequence.Invoke();
    }

    public void StopSubtitles(){        //Stop the subtitle sequence
        DelayManager.instance.CancelInvoke();
    }

    void UpdateTMP(string t){           //update the text string on screen
        tmpText.text = t;
    }

    void SubtitlesFinished(){           //Called at the end of the subtitle sequence
        
        if(loop){
            Debug.Log("Subtitles finished, looping.");      //if loop == true, loop. Otherwise just finish.
            StartSubtitles();
        }
        else{
            Debug.Log("Subtitles finished.");
        }
    }

    public void loadSubtitlesFile(){                    //Loads the JSON file and puts the data into the subs object.
        string jsonString = subtitlesFile.text;
        subs = JsonUtility.FromJson<Subtitles>(jsonString);
    }
}*/
/*
[System.Serializable]
public class Subtitles{         //Data classes to match the format of the JSON file
    public string name;
    public bool usingDuration;
    public List<Line> lines;
}   

[System.Serializable]           //Data subclass
public class Line{
    public string text;
    public float time;
}**/