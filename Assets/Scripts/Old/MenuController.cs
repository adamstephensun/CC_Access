using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour     //Script to control the global menu. Used for testing, not in final product
{
    public GameObject menu;

    void Start(){
        menu.SetActive(false);
    }

    public void ToggleMenu(){
        menu.SetActive(!menu.activeSelf);
    }

    public void ChangeScene(int index){
        if(SceneManager.GetSceneAt(index) != null){
            SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        }
        else{
            Debug.Log("Scene index: " + index + " does not exist.");
        }
    }
}
