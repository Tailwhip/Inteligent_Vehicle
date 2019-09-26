using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltrasonicMeassure : MonoBehaviour {

    private float visibleDistance = 400f;
    public float distance = 0f;
    public Transform ultrasonicSensor;
    
    void FixedUpdate()
    {
        int layerMask = 1 << 14;
        RaycastHit hit;

        if (Physics.Raycast(ultrasonicSensor.transform.position, ultrasonicSensor.transform.forward, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(ultrasonicSensor.transform.position, ultrasonicSensor.transform.forward * hit.distance, Color.red);
            distance = Mathf.Clamp(1 - hit.distance / visibleDistance, 0, 1);
        }
        else
            distance = 0f;
    }  
}
