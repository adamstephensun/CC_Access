using UnityEngine;

public class GyroPosTest : MonoBehaviour        //Script to test gyroscope behaviour for head tracking. Not used in final product.
{
    public Vector3 forceMultiplier = new Vector3(1,1,1);
    Rigidbody rb;

    void Start(){
        //rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Apply forces to an object to match the side-to-side acceleration
        // the user is giving to the device.
        Vector3 force = new Vector3(Input.gyro.userAcceleration.x * forceMultiplier.x , Input.gyro.userAcceleration.y * forceMultiplier.y , Input.gyro.userAcceleration.z * forceMultiplier.z);
        Vector3 force2 = new Vector3(Input.acceleration.x * forceMultiplier.x , Input.acceleration.y * forceMultiplier.y , Input.acceleration.z * forceMultiplier.z);

        if(force.sqrMagnitude > 1) force.Normalize();

        //rb.AddForce(force);
        //force.Normalize();
        Debug.Log(force);

        transform.Translate(force);
    }
}