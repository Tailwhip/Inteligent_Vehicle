using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltrasonicMeassure : MonoBehaviour {

    private float visibleDistance = 105f;
    public double distance = 0f;
    public Transform ultrasonicSensor;
    private Vector3 toObstacle;

    void Update()
    {
        float dist = 0f;
        int layerMask = 1 << 11;
        RaycastHit hit;
        if (Physics.Raycast(ultrasonicSensor.position, ultrasonicSensor.transform.forward, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(ultrasonicSensor.position, ultrasonicSensor.transform.forward * hit.distance, Color.red);
            dist = 1 - hit.distance / visibleDistance;
            //Debug.Log("HIT " + hit.distance);
            //Debug.Log("DISTANCE " + distance);
        }
        dist = Round(dist);
        distance = dist;
    }
    /*
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Obstacle")
        {
            distance = 0;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        int layerMask = 1 << 11;
        RaycastHit hit;
        Vector3 toObstaclee = other.ClosestPoint(ultrasonicSensor.position);
        toObstacle = new Vector3(Mathf.Clamp(ultrasonicSensor.forward.x, -ultrasonicSensor.forward.x * Mathf.Sin(0.5236f), ultrasonicSensor.forward.x * Mathf.Sin(0.5236f)),
            Mathf.Clamp(ultrasonicSensor.forward.y, -ultrasonicSensor.forward.y * Mathf.Cos(0.5236f), ultrasonicSensor.forward.y * Mathf.Cos(0.5236f)), ultrasonicSensor.forward.z);

        if (other.gameObject.tag == "Obstacle" && Physics.Raycast(ultrasonicSensor.position, toObstacle, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(ultrasonicSensor.position, -hit.normal * hit.distance, Color.red);
            //distance = 1 - (toObstacle.magnitude - ultrasonicSensor.position.magnitude) / visibleDistance;
            distance = 1 - hit.distance / visibleDistance;

            //Debug.Log("HIT " + hit.distance);
        }
    }
    */
    float Round(float x)
    {
        return (float)System.Math.Round(x, 2, System.MidpointRounding.AwayFromZero); ;
    }
    /*
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(200, 25, 250, 30), "X: " + toObstacle.x);
        GUI.Label(new Rect(200, 50, 250, 30), "Y: " + toObstacle.y);
        GUI.Label(new Rect(200, 75, 250, 30), "Z: " + toObstacle.z);
    }
    */
}
