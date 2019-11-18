using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerometerMeassure : MonoBehaviour
{
    public Rigidbody meassuredObj;
    private float velMax = 1f; // 39f;
    private float accMax = 1984.05f; // 2400f;

    public float velocityX = 0f;
    public float velocityZ = 0f;

    public float accelerationX = 0f;
    public float accelerationZ = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        /// Counting velocity X and Z
        velocityX = Mathf.Clamp(1 - (meassuredObj.velocity.x / velMax), 0, 1);
        velocityZ = Mathf.Clamp(1 - (meassuredObj.velocity.z / velMax), 0, 1);
     
        /// Counting acceleration X and Z
        accelerationX = Round(acceleration(meassuredObj.velocity.x));
        accelerationZ = Round(acceleration(meassuredObj.velocity.z));
    }

    /// Function for counting acceleration
    private float acceleration(float vel)
    {
        return Mathf.Clamp((vel / Time.fixedDeltaTime + accMax) / (2*accMax), 0, 1);
    }

    float Round(float x)
    {
        return (float)System.Math.Round(x, 2);
    }
}
