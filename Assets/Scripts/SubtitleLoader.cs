using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitleLoader : MonoBehaviour     //This script handles loading subtitle files
{
    public NetworkManager netManager;

    public List<TextAsset> fileList = new List<TextAsset>();
    [HideInInspector] public List<SubGroup> subList = new List<SubGroup>();

    SubtitlePlayer subPlayer;

    void Start()
    {
        subPlayer = GetComponent<SubtitlePlayer>();

        LoadFiles();
    }

    public void LoadFiles(){
        // Loads all the files in fileList, converts them to SubGroup objects and adds them to the subList

        foreach(TextAsset f in fileList){
            string jsonString = f.text;
            subList.Add(JsonUtility.FromJson<SubGroup>(jsonString));
        }

        Debug.Log("Loaded " + fileList.Count + " subtitle files");

        // Notifiy subPlayer and networkManager that subs are loaded
        subPlayer.SubsLoaded(subList);
        netManager.InitListeners();
    }
}

[System.Serializable]
public class Subtitles{         //Data classes to match the format of the JSON file
    public string name;
    public List<string> lines;
}

[System.Serializable]
public class SubGroup{
    public string name;
    public List<Line> captions;
}

[System.Serializable]
public class Line{
    public int duration;
    public string content;
    public bool startOfParagraph;
    public string startTime;
}