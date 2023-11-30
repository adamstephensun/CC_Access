using UnityEngine;

[ExecuteInEditMode]
public class PostProcessing : MonoBehaviour     //Applies the custom shader to each camera view for distortion
{
    public Material mat;

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        Graphics.Blit(src, dest, mat);
    }
}
