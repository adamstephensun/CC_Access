using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataSaver : MonoBehaviour
{
    public Answers _an;
    string dataPath;

    string debugPrefix = "DATA SAVER --- ";

    void Start(){
        _an.answers.Capacity = 10;
    }

    public void AddAnswer(int result){
        _an.answers.Add(result);
        Debug.Log(debugPrefix + "Answer saved: " + result);
    }

    void SetDataPath(){
        DateTime time = DateTime.Now;
        dataPath = Application.persistentDataPath + "/answers" + time.ToString("_yyyyMMdd_HHmmss") + ".json";
    }

    public void ClearAnswers(){
        Debug.Log("Data saver answers cleared");
        _an.answers.Clear();
    }

    public void LastQuestionAnswered(){
        
        if(_an.answers.Count == 10){
            Debug.Log("Last question answered, saving to json");
            SaveToJson();
        }
        else if(_an.answers.Count < 10){
            Debug.Log(_an.answers.Count + " questions have been answered. There must be 10.");
            // If not enough answers in the list, add enough to make it 10
            int answersMissing = 10 - _an.answers.Count;
            for(int i = 0 ; i < answersMissing ; i++){
                _an.answers.Add(UnityEngine.Random.Range(0, 2));
                if(_an.answers.Count == 10) SaveToJson();
            }
        }
    }

    void SaveToJson(){
        SetDataPath();
        string data = JsonUtility.ToJson(_an);
        System.IO.File.WriteAllText(dataPath, data);
        Debug.Log(debugPrefix + "Data saved to json: " + data); 
    }
}

[Serializable]
public class Answers{
    public List<int> answers = new List<int>();
}