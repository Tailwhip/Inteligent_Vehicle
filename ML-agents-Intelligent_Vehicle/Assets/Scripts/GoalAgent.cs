using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GoalAgent : Agent {

    /// Objects:
    private Rigidbody rb;
    public GameObject wall;
    public GameObject wall2;
    public GameObject lightSource;

    /// variables for reset the agent:
    private Quaternion vehicleStartRot;
    private Vector3 vehicleStartPos;
    private Vector3 lightStartPos;
    private float showReward = 0f;
    private Vector3 wallStartPos;
    private Quaternion wallStartRot;
    private Vector3 wall2StartPos;
    private Quaternion wall2StartRot;

    /// Ultrasonic sensors input
    public UltrasonicMeassure ultraSens1;
    public UltrasonicMeassure ultraSens2;
    public UltrasonicMeassure ultraSens3;
    public UltrasonicMeassure ultraSens4;
    public UltrasonicMeassure ultraSens5;

    /// Phototransistor sensors input
    public PhototransistorMeassure phototransistor1;
    public PhototransistorMeassure phototransistor2;
    public PhototransistorMeassure phototransistor3;
    public PhototransistorMeassure phototransistor4;
    
    /// Velocity input
    public AccelerometerMeassure accelerometer;
    List<float> velX = new List<float>();
    List<float> velZ = new List<float>();

    /// Variables for saving vector observation:
    List<float> US1dist = new List<float>();
    List<float> US2dist = new List<float>();
    List<float> US3dist = new List<float>();
    List<float> US4dist = new List<float>();
    List<float> US5dist = new List<float>();

    List<float> intens1 = new List<float>();
    List<float> intens2 = new List<float>();
    List<float> intens3 = new List<float>();
    List<float> intens4 = new List<float>();

    List<float> Q1 = new List<float>();
    List<float> Q2 = new List<float>();

    /// Variables to count delta intensity:
    private float deltaIntensity;
    private float intensityOld = 0f;
    private float intensity = 0f;
    private int deltaCounter = 1;

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        /// initialise delta intensity variables:
        intensity = phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity;
        intensityOld = 0.0f;

        /// initialise reset position variables:
        rb = this.GetComponent<Rigidbody>();
        vehicleStartRot = this.transform.rotation;
        vehicleStartPos = this.transform.position;
        lightStartPos = lightSource.transform.position;

        wallStartPos = wall.transform.position;
        wallStartRot = wall.transform.rotation;
    
        wall2StartPos = wall2.transform.position;
        wall2StartRot = wall2.transform.rotation;
    }

    ///Reward conditions:
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(-1.0f);
            Done();
        }

        if (collision.gameObject.tag == "ResetPlane")
        {
            AgentReset();
        }
    }

    /// Collecting Input:
    public override void CollectObservations()
    {
        AddVectorObs(ultraSens1.distance);
        AddVectorObs(ultraSens2.distance);
        AddVectorObs(ultraSens3.distance);
        AddVectorObs(ultraSens4.distance);
        AddVectorObs(ultraSens5.distance);

        AddVectorObs(accelerometer.velocityX);
        AddVectorObs(accelerometer.velocityZ);

        AddVectorObs(phototransistor1.intensity);
        AddVectorObs(phototransistor2.intensity);
        AddVectorObs(phototransistor3.intensity);
        AddVectorObs(phototransistor4.intensity);

        intensity = phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity;
        /*
        /// for saving vector observation:
        US1dist.Add(US1distance);
        US2dist.Add(US2distance);
        US3dist.Add(US3distance);
        US4dist.Add(US4distance);
        US5dist.Add(US5distance);
        velX.Add(rb.velocity.x / velMax);
        velZ.Add(rb.velocity.z / velMax);
        intens1.Add(phototransistor1.intensity);
        intens2.Add(phototransistor2.intensity);
        intens3.Add(phototransistor3.intensity);
        intens4.Add(phototransistor4.intensity);
        */
    }

    /// Moving agent with outputs:
    public override void AgentAction(float[] act, string textAction)
    {
        base.AgentAction(act, textAction);
                       
        /// counting delta intensity and use it to punish or reward:
        deltaCounter--;
        if (deltaCounter == 0)
        {
            deltaCounter = 1;
            deltaIntensity =  intensity - intensityOld;
            if (deltaIntensity <= 0.01f)
            {
                AddReward(-0.005f);
            }
            else
            {
                AddReward(0.005f);
            }
            intensityOld = intensity;
        }
        
        /// for delta distance input: 
        if (intensity > 3.5f)
        {
            SetReward(1f);
            Done();
        }

        ///add position and rotation:
        rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 300f);
        this.transform.Rotate(0, Mathf.Clamp(act[1], -1f, 1f) * 2f, 0, 0);

        ///for saving vector observations:
        Q1.Add(act[0]);
        Q2.Add(act[1]);
    }

    public override void AgentReset()
    {
        base.AgentReset();
        showReward = GetCumulativeReward();
        this.transform.position = vehicleStartPos + new Vector3(Random.Range(-20, 20), 0, (Random.Range(-5, 5)));
        this.transform.rotation = vehicleStartRot;
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
        lightSource.transform.position = lightStartPos + new Vector3(Random.Range(-20, 20), 0, (Random.Range(-20, 10)));
        wall.transform.position = wallStartPos + new Vector3(Random.Range(-10, 10), 0, 0);
        wall.transform.Rotate(0f, Random.Range(-180, 180), 0f, 0);
        wall.transform.localScale = new Vector3(Random.Range(10, 25), 15, 5);
        wall2.transform.position = wall2StartPos + new Vector3(Random.Range(-10, 10), 0, 0);
        wall2.transform.Rotate(0f, Random.Range(-180, 180), 0f, 0);
        wall2.transform.localScale = new Vector3(Random.Range(10, 25), 15, 5);
        //deltaCounter = 20;
        intensityOld = 0.0f;
        /*
        //SaveToFile(US1dist, US2dist, US3dist, US4dist, US5dist, velX, velZ, intens1, intens2, intens3, intens4, Q1, Q2, "TestValues");

        US1dist.Clear();
        US2dist.Clear();
        US3dist.Clear();
        US4dist.Clear();
        US5dist.Clear();
        velX.Clear();
        velZ.Clear();
        intens1.Clear();
        intens2.Clear();
        intens3.Clear();
        intens4.Clear();
        Q1.Clear();
        Q2.Clear();
        */
    }

    public void SaveToFile(List<float> US1dist, List<float> US2dist, List<float> US3dist, List<float> US4dist, List<float> US5dist,
        List<float> velX, List<float> velZ, List<float> intens1, List<float> intens2, List<float> intens3, List<float> intens4, List<float> Q1, List<float> Q2, string filename)
    {
        string data = "";
        for (int i = 0; i < US1dist.Count; i++)
        {
            data += i + ";" + US1dist[i] + ";" + US2dist[i] + ";" + US3dist[i] + ";" + US4dist[i] + ";" + US5dist[i] + ";" + velX[i] + ";" + velZ[i] +
                ";" + intens1[i] + ";" + intens2[i] + ";" + intens3[i] + ";" + intens4[i] + ";" + Q1[i] + ";" + Q2[i] + System.Environment.NewLine;
        }
        string path = Application.dataPath + "/" + filename + ".txt";
        StreamWriter wf = File.CreateText(path);
        wf.WriteLine(data);
        wf.Close();
        Debug.Log("SAVED");
    }

    // Showing ANN data:
    private void OnGUI()
    {
        //myGUI();
    }

    private void myGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(25, 25, 250, 30), "US1: " + ultraSens1.distance);
        GUI.Label(new Rect(25, 50, 250, 30), "US2: " + ultraSens2.distance);
        GUI.Label(new Rect(25, 75, 250, 30), "US3: " + ultraSens3.distance);
        GUI.Label(new Rect(25, 100, 250, 30), "US4: " + ultraSens4.distance);
        GUI.Label(new Rect(25, 125, 250, 30), "US5: " + ultraSens5.distance);

        GUI.color = Color.yellow;
        GUI.Label(new Rect(300, 25, 250, 30), "Velocity X: " + accelerometer.velocityX);
        GUI.Label(new Rect(300, 50, 250, 30), "Velocity Z: " + accelerometer.velocityZ);

        GUI.color = Color.blue;
        GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + phototransistor1.intensity);
        GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + phototransistor2.intensity);
        GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + phototransistor3.intensity);
        GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + phototransistor4.intensity);

        GUI.color = Color.green;
        GUI.Label(new Rect(500, 25, 250, 30), "delta intensity: " + deltaIntensity);
        GUI.Label(new Rect(500, 50, 250, 30), "Intensity: " + intensity);
    }
}
