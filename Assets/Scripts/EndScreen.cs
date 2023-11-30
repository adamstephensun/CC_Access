using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.Security.Cryptography;

public class EndScreen : MonoBehaviour
{

    public DataSaver dataSaver; 

    public List<TextAsset> sampleData = new List<TextAsset>();

    [Header("UI elements")]
    public List<TextMeshProUGUI> youAnsweredText = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> yesPercentText = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> noPercentText = new List<TextMeshProUGUI>();

    List<Answers> answerList = new List<Answers>();

    public List<int> numYes = new List<int>();
    List<string> yesPercentValues = new List<string>();
    List<string> noPercentValues = new List<string>();

    List<int> currentAnswers = new List<int>();

    public void InitialiseEndScreen(){

        if(numYes.Count == 0){
            for(int i = 0 ; i < 10 ; i++){
                numYes.Add(0);
            }
        }

        LoadData();
        CalculateValues();
        PopulateUI();
    }

    void PopulateUI(){
        for(int i = 0 ; i < 10 ; i++){

            if(currentAnswers[i] == 0) youAnsweredText[i].text = "You answered: no";
            else if(currentAnswers[i] == 1) youAnsweredText[i].text = "You answered: yes";

            yesPercentText[i].text = yesPercentValues[i] + "% said yes";
            noPercentText[i].text = noPercentValues[i] + "% said no";
        }
    }

    void CalculateValues(){
        
        //Set numYes list to default value of 0
        for(int i = 0 ; i < 10 ; i++) numYes[i] = 0;

        // Get the number of yes answers per question
        for(int i = 0 ; i < answerList.Count ; i++){
            for(int j = 0 ; j < 10 ; j++){
                if(answerList[i].answers[j] == 1){
                    //Debug.Log("Postive at i = " + i + "   j = " + j);
                    numYes[j]++;
                }

                if(i == answerList.Count - 1) currentAnswers = answerList[i].answers;
            }
        }

        // Calculate percentages and fill percentage lists
        for(int i = 0 ; i < 10 ; i++){
            float val = ((float)numYes[i] / (float)answerList.Count) * 100;
            //Debug.Log("numYes[" + i + "] (" + numYes[i] + ") / answerList.Count (" + answerList.Count + ") = " + val);
            yesPercentValues.Add(val.ToString("0."));
            noPercentValues.Add((100 - val).ToString("0."));
        }
    }

    void LoadData(){
        if(Directory.Exists(Application.persistentDataPath)){
            answerList.Clear();
            yesPercentValues.Clear();
            noPercentValues.Clear();

            DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);

            foreach(var file in d.GetFiles()){
                StreamReader sr = file.OpenText();
                
                string jsonText = sr.ReadToEnd();

                Debug.Log("File read: " + jsonText);

                Answers ans = JsonConvert.DeserializeObject<Answers>(jsonText);
                answerList.Add(ans);
            }
        }
    }
}
