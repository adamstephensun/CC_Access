using UnityEngine;
using TMPro;

public class GyroTest : MonoBehaviour       //Script to test gyroscope behaviour for head tracking. Not used in final product.
{
    public TextMeshProUGUI gyroText;

    // Start is called before the first frame update
    void Start()
    {
        if(SystemInfo.supportsGyroscope) Debug.Log("Gyro supported");
        else Debug.Log("Gyro not supported");

        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = GyroToUnity(Input.gyro.attitude);
        gyroText.text = "Attitude: " + Input.gyro.attitude;

        Debug.Log("Gyro enabled: " + Input.gyro.enabled.ToString());
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
