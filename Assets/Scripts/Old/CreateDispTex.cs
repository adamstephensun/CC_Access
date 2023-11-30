using UnityEngine;

public class CreateDispTex : MonoBehaviour  //test script to generate a displacement texture for screen distortion. Not used in final product
{
    int width = 785;
    int height = 844;

    Texture2D displacementTex;

    Color[] halfRow;

    void Start()
    {
        Debug.Log("Texture gen start");
        halfRow = new Color[width / 2];

        displacementTex = new Texture2D(width,height);

        for(int i = 0 ; i < width / 2 ; i++){ 
            //float g = ;

            //halfRow[i].g = g;
        }

        foreach(var col in halfRow){
            Debug.Log(col.g);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
