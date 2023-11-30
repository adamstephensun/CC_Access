using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UserInputManager : MonoBehaviour
{
    [Header("Object references")]
    public GameObject inputUI;
    public GameObject giveLifeUI;
    public GameObject endScreenObj;
    public TextMeshProUGUI questionText;
    public Slider progressSlider;

    public Transmitter transmitter;
    public AudioSource audioSource;
    public ADPlayer adPlayer;
    public BSLPlayer bslPlayer;
    public DataSaver dataSaver;

    [Header("Variables")]
    public Color col;

    public float hueIncreasePc;
    public float satIncreasePc;
    public float timeToAnswer;

    public float minHue = 150;
    public float maxHue = 330;

    public float randHueVariance = 0.2f;
    public float satIncrease = 0.09f;

    [Header("Audio")]
    public AudioClip yesClip;
    public AudioClip noClip;

    [Header("Lists")]
    public List<string> questions = new List<string>();

    //Private
    bool questionActive = false;
    bool giveLifeAnswered = false;
    public bool lastQuestion = false;
    
    int currentQuestionID;
    float maxGiveLifeWaitTime = 8;
    float totalGiveLifeTime = 18;
    float t = 0;
    float maxHueNormalised;
    float minHueNormalised;

    string debugPrefix = "INPUT MANAGER --- ";


    void Start()
    {
        inputUI.SetActive(false);
        giveLifeUI.SetActive(false);

        col = UnityEngine.Random.ColorHSV(0, 1, 0.0f, 0.01f, 0.99f, 1);     // set random start colour, may need to change this
        float _h, _s, _v;
        Color.RGBToHSV(col, out _h, out _s, out _v);
        //Debug.Log("Col randomly initialised to: h" + _h + "  s" + _s + "  v" + _v);
        
        progressSlider.maxValue = timeToAnswer;
        progressSlider.minValue = 0;

        maxHueNormalised = ConvertRange(maxHue, 0, 360, 0, 1);
        minHueNormalised = ConvertRange(minHue, 0, 360, 0, 1);
    }

    void Update(){
        if(questionActive){
            if(t < timeToAnswer){
                progressSlider.value = Mathf.Lerp(0, timeToAnswer, t / timeToAnswer);
                t += Time.deltaTime;
            }
            else{
                progressSlider.value = timeToAnswer;

                Debug.Log(debugPrefix + "Question timed out, auto answering no.");

                NoInput();
                questionActive = false;
            }
        }
    }

    public void YesInput(){
        // increase hue by value%, increase sat by value%
        float _h, _s, _v;
        Color.RGBToHSV(col, out _h, out _s, out _v);

        float randVariance = UnityEngine.Random.Range(-randHueVariance, randHueVariance);

        _h = _h + (_h * (hueIncreasePc + randVariance));     // maybe add randomness here so increase isn't always 60%
        _s += satIncrease;

        if(_h > maxHueNormalised){
            //Debug.Log(debugPrefix + "h:" + _h + " too high, wrapping around to " + (minHueNormalised + (_h - maxHueNormalised)));
            _h = minHueNormalised + (_h - maxHueNormalised);     // wrap around hue if over than 1
        }
        if(_s > 1) _s = 1;

        col = Color.HSVToRGB(_h, _s, _v);

        //Debug.Log(debugPrefix + "Randomised color to - h" + _h + "  s" + _s + "  v" + _v);

        // tell transmitter to send color to lighting desk
        transmitter.SendInputMessage(col, 5);

        // disable input UI
        inputUI.SetActive(false);
        questionActive = false;

        AudioSource.PlayClipAtPoint(yesClip, Vector3.zero);

        DateTime date = DateTime.Now;
        string prefsString = date + " - Q" + currentQuestionID;
        Debug.Log(debugPrefix + "Saving 1 to player prefs key: " + prefsString);
        PlayerPrefs.SetInt(prefsString, 1);
        string shortPrefsKey = "Q" + currentQuestionID;
        PlayerPrefs.SetInt(shortPrefsKey, 1);

        if(currentQuestionID == 0) dataSaver.ClearAnswers();

        dataSaver.AddAnswer(1);

        if(currentQuestionID == 9) dataSaver.LastQuestionAnswered();
    }

    public void NoInput(){
        // increase saturation buy value%
        float _h, _s, _v;
        Color.RGBToHSV(col, out _h, out _s, out _v);

        _h = _h + (_h * hueIncreasePc / 2);
        _s = _s + (_s * satIncreasePc);

        if(_h > 1) _h -= 1;     // wrap around hue if over than 1
        if(_s > 0.9f) _s = 0.9f;

        // if hue is outside of min or max, set to min or max
        if(_h > maxHueNormalised) _h = maxHueNormalised;
        if(_h < minHueNormalised) _h = minHueNormalised;

        col = Color.HSVToRGB(_h, _s, _v);

        // tell transmitter to send color to lighting desk
        transmitter.SendInputMessage(col, 5);

        // disable input UI
        inputUI.SetActive(false);
        questionActive = false;

        // Stop AD and play feedback clip
        //adPlayer.StopClip();
        AudioSource.PlayClipAtPoint(noClip, Vector3.zero);

        DateTime date = DateTime.Now;
        string prefsString = date + " - Q" + currentQuestionID;
        Debug.Log(debugPrefix + "Saving 0 to player prefs key: " + prefsString);
        PlayerPrefs.SetInt(prefsString, 0);
        string shortPrefsKey = "Q" + currentQuestionID;
        PlayerPrefs.SetInt(shortPrefsKey, 0);

        if(currentQuestionID == 0) dataSaver.ClearAnswers();

        dataSaver.AddAnswer(0);

        if(currentQuestionID == 9) dataSaver.LastQuestionAnswered();
    }

    public void GiveLife(){
        giveLifeAnswered = true;
        giveLifeUI.SetActive(false);
        transmitter.SendInputMessage(col, 20);
    }

    public void ShowGiveLife(){
        float delay = transmitter.deviceID / 2.5f;
        Debug.Log("Random give life delay: " + delay);

        StartCoroutine(GiveLifeDelay(delay));
    }

    IEnumerator GiveLifeDelay(float initialDelay){
        adPlayer.PauseClip();
        Time.timeScale = 0;
        Debug.Log("Time scale set to 0 then waiting " + initialDelay + " seconds to show give life message");

        yield return new WaitForSecondsRealtime(initialDelay);

        giveLifeUI.SetActive(true);
        giveLifeAnswered = false;

        float extraTime = (totalGiveLifeTime - maxGiveLifeWaitTime) + (maxGiveLifeWaitTime - initialDelay);
        Debug.Log("Waiting for " + extraTime + "  total: " + (extraTime + initialDelay));

        yield return new WaitForSecondsRealtime(extraTime);

        if(!giveLifeAnswered){
            Debug.Log("Give life timed out");
            GiveLife();
        }

        Debug.Log("Time scale set to 1");
        Time.timeScale = 1;
        adPlayer.UnPauseClip();
    }

    public void ShowRandomQuestionPrompt(){
        int randQuestionNum;

        if(questions.Count > 0) randQuestionNum = UnityEngine.Random.Range(0, questions.Count);
        else randQuestionNum = 0;

        ShowInputPrompt(randQuestionNum);
    }

    public void ShowInputPrompt(int questionNum){

        if(questionNum <= questions.Count){
            if(questions[questionNum] != null) questionText.text = questions[questionNum];
            else questionText.text = "No question for trigger:" + questionNum.ToString();
        }
        else{
            Debug.LogError(debugPrefix + "Question num: " + questionNum + " is out of bounds");
        }

        inputUI.SetActive(true);

        timeToAnswer = adPlayer.PlayQuestionLine(questionNum);

        progressSlider.maxValue = timeToAnswer;
        Debug.Log(debugPrefix + "Question time set to " + timeToAnswer);

        //Reset and initialise slider and lerp
        t = 0;
        progressSlider.value = 0;
        questionActive = true;
        currentQuestionID = questionNum;

        if(questionNum == questions.Count - 1){
            lastQuestion = true;
        }
    }

    public void ShowEndScreen(){
        endScreenObj.SetActive(true);

        EndScreen end = endScreenObj.GetComponent<EndScreen>();
        end.InitialiseEndScreen();
    }

    public void ClearInputPrompt(){
        if(questionActive){
            audioSource.Stop();
            questionActive = false;

            inputUI.SetActive(false);
        }
    }

    public int getNumOfQuestions(){
        return questions.Count;
    }

    float ConvertRange(float val, float oldMin, float oldMax, float newMin, float newMax){
        // Converts a value within a range to a new range
        return (( val - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
    }
}
