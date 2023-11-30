using System.Collections;
using UnityEngine;

public class CoRunner : MonoBehaviour
{
    public void Run(IEnumerator cor)
    {
        StartCoroutine(cor);
    }
}