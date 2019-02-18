using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GoalAgent : Agent {

    // Wheels input
    public WheelCollider rightWheel_C, leftWheel_C, backWheel_C;
    public Transform rightWheel_T, leftWheel_T, backWheel_T;
    public GameObject LightSource;

    // Objects:
    private Rigidbody rb;
    public GameObject wall;

    // variables for reset the agent:
    private Quaternion vehicleStartRot;
    private Vector3 vehicleStartPos;
    private Vector3 lightStartPos;
    private float looseCount = 0f;
    private float winCount = 0f;
    private float showReward = 0f;
    private Vector3 wallStartPos;

    // Ultrasonic sensors input
    public UltrasonicMeassure ultraSens1;
    public UltrasonicMeassure ultraSens2;
    public UltrasonicMeassure ultraSens3;
    public UltrasonicMeassure ultraSens4;
    public UltrasonicMeassure ultraSens5;

    // Phototransistor sensors input 
    public PhototransistorMeassure phototransistor1;
    public PhototransistorMeassure phototransistor2;
    public PhototransistorMeassure phototransistor3;
    public PhototransistorMeassure phototransistor4;

    // variables to count delta intensity:
    private float deltaIntensity;
    private float intensityOld = 0f;
    private float intensity = 0f;
    private int deltaCounter = 1;

    // light intensity input:
    private Vector2 intensity1;
    private Vector2 intensity2;
    private Vector2 intensity3;
    private Vector2 intensity4;

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        // initialise delta intensity variables:
        intensity = (1f - (intensity1.magnitude / 100f)) + (1f - (intensity2.magnitude / 100f)) + (1f - (intensity3.magnitude / 100f)) + (1f - (intensity4.magnitude / 100f));
        intensityOld = 0.0f;

        // initialise reset position variables:
        rb = this.GetComponent<Rigidbody>();
        vehicleStartRot = this.transform.rotation;
        vehicleStartPos = this.transform.position;
        lightStartPos = LightSource.transform.position;
        wallStartPos = wall.transform.position;
    }

    //Reward conditions:
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            SetReward(-1.0f);
            Done();
        }

        if (collision.gameObject.tag == "ResetPlane")
        {
            AgentReset();
        }
    }

    // Collecting Input:
    public override void CollectObservations()
    {
        AddVectorObs(ultraSens1.distance);
        AddVectorObs(ultraSens2.distance);
        AddVectorObs(ultraSens3.distance);
        AddVectorObs(ultraSens4.distance);
        AddVectorObs(ultraSens5.distance);
        AddVectorObs(ultraSens1.deltaDistance);
        AddVectorObs(ultraSens2.deltaDistance);
        AddVectorObs(ultraSens3.deltaDistance);
        AddVectorObs(ultraSens4.deltaDistance);
        AddVectorObs(ultraSens5.deltaDistance);
        /*
        AddVectorObs((1f - phototransistor1.intensity) * 100f);
        AddVectorObs((1f - phototransistor2.intensity) * 100f);
        AddVectorObs((1f - phototransistor3.intensity) * 100f);
        AddVectorObs((1f - phototransistor4.intensity) * 100f);
        */
        
        AddVectorObs(1f - (intensity1.magnitude / 100f));
        AddVectorObs(1f - (intensity2.magnitude / 100f));
        AddVectorObs(1f - (intensity3.magnitude / 100f));
        AddVectorObs(1f - (intensity4.magnitude / 100f));
        /*
        AddVectorObs(intensity1.magnitude);
        AddVectorObs(intensity2.magnitude);
        AddVectorObs(intensity3.magnitude);
        AddVectorObs(intensity4.magnitude);
        */
    }

    // Moving agent with outputs:
    public override void AgentAction(float[] act, string textAction)
    {
        base.AgentAction(act, textAction);
        
        intensity1.Set((LightSource.transform.position.x - phototransistor1.transform.position.x), (LightSource.transform.position.z - phototransistor1.transform.position.z));
        intensity2.Set((LightSource.transform.position.x - phototransistor2.transform.position.x), (LightSource.transform.position.z - phototransistor2.transform.position.z));
        intensity3.Set((LightSource.transform.position.x - phototransistor3.transform.position.x), (LightSource.transform.position.z - phototransistor3.transform.position.z));
        intensity4.Set((LightSource.transform.position.x - phototransistor4.transform.position.x), (LightSource.transform.position.z - phototransistor4.transform.position.z));

        intensity = (1f - (intensity1.magnitude / 100f)) + (1f - (intensity2.magnitude / 100f)) + (1f - (intensity3.magnitude / 100f)) + (1f - (intensity4.magnitude / 100f));
        
        deltaCounter--;
        // counting delta intensity and use it to punish or reward:
        if (deltaCounter == 0)
        {
            deltaCounter = 1;
            deltaIntensity = intensity - intensityOld;
            if (deltaIntensity <= 0.02f)
            {
                AddReward(-0.005f);
                //Debug.Log("LOOSE1");
                looseCount++;
            }

            /*
            else
            {
                AddReward(0.005f);
                Debug.Log("WIN1");
                winCount++;
            }
            */
            intensityOld = intensity;
        }
        
        AddReward(-0.005f);
        // for delta distance input: 
        if (intensity > 3.7f)
        {
            SetReward(1f);
            Done();
        }
        
        /*
        // for PTs input:
        if (intensity < 80f)
        {
            SetReward(1f);
            Done();
        }
        */
        //add position and rotation:
        this.transform.position += this.transform.forward * Mathf.Clamp(act[0], 0f, 1f) * 2f;
        this.transform.Rotate(0, Mathf.Clamp(act[1], -1f, 1f) * 2f, 0, 0);
    }

    public override void AgentReset()
    {
        base.AgentReset();
        showReward = GetCumulativeReward();
        this.transform.position = vehicleStartPos + new Vector3(Random.Range(-20, 20), 0, (Random.Range(-5, 5)));
        this.transform.rotation = vehicleStartRot;
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
        LightSource.transform.position = lightStartPos + new Vector3(Random.Range(-20, 20), 0, (Random.Range(-20, 10)));
        wall.transform.position = wallStartPos + new Vector3(Random.Range(-10, 10), 0, 0);
        deltaCounter = 20;
        intensityOld = 0.0f;

        ultraSens1.deltaCounter = 20;
        ultraSens2.deltaCounter = 20;
        ultraSens3.deltaCounter = 20;
        ultraSens4.deltaCounter = 20;
        ultraSens5.deltaCounter = 20;
        ultraSens1.distanceOld = 0.0f;
        ultraSens2.distanceOld = 0.0f;
        ultraSens3.distanceOld = 0.0f;
        ultraSens4.distanceOld = 0.0f;
        ultraSens5.distanceOld = 0.0f;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(rightWheel_C, rightWheel_T);
        UpdateWheelPose(leftWheel_C, leftWheel_T);
        UpdateWheelPose(backWheel_C, backWheel_T);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        Vector3 pos = transform.position;
        Quaternion quat = transform.rotation;

        collider.GetWorldPose(out pos, out quat);
        transform.position = pos;
        transform.rotation = quat;
    }

    // Showing ANN data:
    private void OnGUI()
    {
        /*
        GUI.color = Color.red;
        GUI.Label(new Rect(25, 25, 250, 30), "US1: " + ultraSens1.distance);
        GUI.Label(new Rect(25, 50, 250, 30), "US2: " + ultraSens2.distance);
        GUI.Label(new Rect(25, 75, 250, 30), "US3: " + ultraSens3.distance);
        GUI.Label(new Rect(25, 100, 250, 30), "US4: " + ultraSens4.distance);
        GUI.Label(new Rect(25, 125, 250, 30), "US5: " + ultraSens5.distance);
        GUI.color = Color.blue;
        GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + phototransistor1.intensity);
        GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + phototransistor2.intensity);
        GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + phototransistor3.intensity);
        GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + phototransistor4.intensity);
        GUI.color = Color.yellow;
        GUI.Label(new Rect(300, 25, 250, 30), "US1 velocity: " + ultraSens1.deltaDistance);
        GUI.Label(new Rect(300, 50, 250, 30), "US2 velocity: " + ultraSens2.deltaDistance);
        GUI.Label(new Rect(300, 75, 250, 30), "US3 velocity: " + ultraSens3.deltaDistance);
        GUI.Label(new Rect(300, 100, 250, 30), "US4 velocity: " + ultraSens4.deltaDistance);
        GUI.Label(new Rect(300, 125, 250, 30), "US5 velocity: " + ultraSens5.deltaDistance);
        
        GUI.color = Color.blue;
        GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + (1f - (intensity1.magnitude / 100f)));
        GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + (1f - (intensity2.magnitude / 100f)));
        GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + (1f - (intensity3.magnitude / 100f)));
        GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + (1f - (intensity4.magnitude / 100f)));
        /*
        GUI.color = Color.green;
        GUI.Label(new Rect(500, 25, 250, 30), "delta intensity: " + deltaIntensity);
        GUI.Label(new Rect(500, 50, 250, 30), "reward: " + showReward);
        GUI.Label(new Rect(500, 75, 250, 30), "Intensity: " + intensity);
        
        if (looseCount > 0)
        GUI.Label(new Rect(500, 150, 250, 30), "WIN/LOOSE: " + winCount / looseCount);
        */
    }
}
