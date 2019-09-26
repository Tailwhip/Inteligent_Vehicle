using MLAgents;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GoalAgent : Agent {

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

    /// Variables to count delta intensity:
    private float deltaIntensity;
    private float intensityOld = 0f;
    private float intensity = 0f;
    private int deltaCounter = 1;

    /// Objects:
    private Rigidbody rb;
    public GameObject wall;
    public GameObject wall2;
    public GameObject wall3;
    public GameObject lightSource;

    /// variables for reset the agent:
    private Vector3 vehicleStartPos;
    private Quaternion vehicleStartRot;
    private Vector3 lightStartPos;
    private Vector3 wallStartPos;
    private Quaternion wallStartRot;
    private Vector3 wall2StartPos;
    private Quaternion wall2StartRot;
    private Vector3 wall3StartPos;
    private Quaternion wall3StartRot;

    /// Variables for creating a training dataset for Pi_Intelligent_Vehicle
    public bool saveTrainSet = false;
    private List<float> vectorActions = new List<float>();
    ///float episode_returns = 0;
    ///float reward = 0;
    private List<float> vectorObs = new List<float>();
    private int episodeStarts = 0;
    private int episodeStartsCounter = 999;
    

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        /// initialise delta intensity variables:
        intensity = phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity;

        /// initialise reset position variables:
        rb = this.GetComponent<Rigidbody>();
        vehicleStartRot = this.transform.rotation;
        vehicleStartPos = this.transform.position;
        lightStartPos = lightSource.transform.position;

        wallStartPos = wall.transform.position;
        wallStartRot = wall.transform.rotation;
    
        wall2StartPos = wall2.transform.position;
        wall2StartRot = wall2.transform.rotation;

        wall3StartPos = wall3.transform.position;
        wall3StartRot = wall3.transform.rotation;

        episodeStarts = 0;
        episodeStartsCounter = 999;
    }

    /// Reward conditions:
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(-1.0f);
            episodeStarts = 1;
            Done();
        }

        if (collision.gameObject.tag == "ResetPlane")
        {
            AgentReset();
            episodeStarts = 1;
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

        AddVectorObs(accelerometer.accelerationX);
        AddVectorObs(accelerometer.accelerationZ);

        AddVectorObs(phototransistor1.intensity);
        AddVectorObs(phototransistor2.intensity);
        AddVectorObs(phototransistor3.intensity);
        AddVectorObs(phototransistor4.intensity);

        intensity = phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity;

        /// for saving vector observation:
        vectorObs.Add(ultraSens1.distance);
        vectorObs.Add(ultraSens2.distance);
        vectorObs.Add(ultraSens3.distance);
        vectorObs.Add(ultraSens4.distance);
        vectorObs.Add(ultraSens5.distance);
        vectorObs.Add(accelerometer.accelerationX);
        vectorObs.Add(accelerometer.accelerationZ);
        vectorObs.Add(phototransistor1.intensity);
        vectorObs.Add(phototransistor2.intensity);
        vectorObs.Add(phototransistor3.intensity);
        vectorObs.Add(phototransistor4.intensity);
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
            if (deltaIntensity <= 0.001f)
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
        if (intensity > 1.7f)
        {
            SetReward(1f);
            episodeStarts = 1;
            Done();
        }

        /// add position and rotation:
        rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], 0f, 1f) * 1500f);
        this.transform.Rotate(0, Mathf.Clamp(act[1], -1f, 1f) * 2f, 0, 0);

        /// for saving vector observations:
        if (saveTrainSet)
        {
            vectorActions.Add(act[0]);
            vectorActions.Add(act[1]);

            episodeStartsCounter--;
            if (episodeStartsCounter == 0)
            {
                episodeStartsCounter = 999;
                episodeStarts = 1;
            }
            Debug.Log(episodeStarts);
            SaveToFile(vectorActions, GetCumulativeReward(), GetReward(), vectorObs, episodeStarts);

            vectorActions.Clear();
            vectorObs.Clear();
            episodeStarts = 0;
        }
    }

    public override void AgentReset()
    {
        base.AgentReset();
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);
        this.transform.position = vehicleStartPos + new Vector3(UnityEngine.Random.Range(-20, 20), 0, (UnityEngine.Random.Range(-5, 5)));
        this.transform.rotation = vehicleStartRot;
        lightSource.transform.position = lightStartPos + new Vector3(UnityEngine.Random.Range(-200, 200), 0, (UnityEngine.Random.Range(-20, 10)));
        wall.transform.position = wallStartPos + new Vector3(UnityEngine.Random.Range(-50, 50), 0, 0);
        wall.transform.Rotate(0f, UnityEngine.Random.Range(-180, 180), 0f, 0);
        wall.transform.localScale = new Vector3(UnityEngine.Random.Range(100, 200), 100, 50);
        wall2.transform.position = wall2StartPos + new Vector3(UnityEngine.Random.Range(-5, 5), 0, 0);
        wall2.transform.Rotate(0f, UnityEngine.Random.Range(-180, 180), 0f, 0);
        wall2.transform.localScale = new Vector3(UnityEngine.Random.Range(10, 25), 15, 5);
        wall3.transform.position = wall3StartPos + new Vector3(UnityEngine.Random.Range(-5, 5), 0, 0);
        wall3.transform.Rotate(0f, UnityEngine.Random.Range(-180, 180), 0f, 0);
        wall3.transform.localScale = new Vector3(UnityEngine.Random.Range(10, 25), 15, 5);
        intensityOld = 0.0f;
    }

    /// Showing ANN data:
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
        GUI.Label(new Rect(300, 25, 250, 30), "Acceleration X: " + accelerometer.accelerationX);
        GUI.Label(new Rect(300, 50, 250, 30), "Acceleration Z: " + accelerometer.accelerationZ);

        GUI.color = Color.blue;
        GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + phototransistor1.intensity);
        GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + phototransistor2.intensity);
        GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + phototransistor3.intensity);
        GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + phototransistor4.intensity);

        GUI.color = Color.green;
        GUI.Label(new Rect(500, 25, 250, 30), "delta intensity: " + deltaIntensity);
        GUI.Label(new Rect(500, 50, 250, 30), "Intensity: " + intensity);
    }

    public void SaveToFile(List<float> actions, float episode_returns, float reward, 
        List<float> obs, int episode_starts)
    {
        /// a dataset buffer
        string data = "";

        data += "a";
        /// adding actions to dataset buffer
        for (int i = 0; i < actions.Count; i++)
        {
            data += actions[i] + ";" ;
        }
        /// delete last "; "
        // data = data.Remove(data.Length - 1, 1);
        // data = data.Remove(data.Length - 1, 1);
        data += ">";

        /// adding episode_return and reward value to the dataset buffer
        data += "e" + episode_returns + ";>r" + reward + ";>" ;

        data += "o";
        /// adding observations to the dataset buffer
        for (int i = 0; i < obs.Count; i++)
        {
            data += obs[i] + ";";
        }
        /// delete last "; "
        // data = data.Remove(data.Length - 1, 1);
        // data = data.Remove(data.Length - 1, 1);
        data += ">";

        /// adding episode_starts to the dataset buffer
        data += "t" + episode_starts + ";>";

        /// adding a new line of dataset
        // data += System.Environment.NewLine;

        string path = @"E:/Zajecia/Projekty/14-Inteligentny_pojazd/03-Oprogramowanie/Pi_Intelligent_vehicle/expert_intelligent_vehicle.txt";

        /// rewriting dataset buffer as a new line of data to the text file
        if (!File.Exists(path))
        {
            StreamWriter wf = File.CreateText(path);
            wf.WriteLine(data);
            wf.Close();
        }
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine(data);
            sw.Close();
        }
    }
}
