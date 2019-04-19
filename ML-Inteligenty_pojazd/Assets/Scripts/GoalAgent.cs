using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GoalAgent : Agent {

    /// Wheels input
    public WheelCollider rightWheel_C, leftWheel_C, backWheel_C;
    public Transform rightWheel_T, leftWheel_T, backWheel_T;
    public GameObject LightSource;

    /// Objects:
    private Rigidbody rb;
    public GameObject wall;
    public GameObject wall2;

    /// variables for reset the agent:
    private Quaternion vehicleStartRot;
    private Vector3 vehicleStartPos;
    private Vector3 lightStartPos;
    private float looseCount = 0f;
    private float winCount = 0f;
    private float showReward = 0f;
    private Vector3 wallStartPos;
    private Quaternion wallStartRot;
    private Vector3 wall2StartPos;
    private Quaternion wall2StartRot;

    /// Ultrasonic sensors input
    public UltrasonicMeassure ultraSens1;
    private Rigidbody rbUS1;
    public UltrasonicMeassure ultraSens2;
    private Rigidbody rbUS2;
    public UltrasonicMeassure ultraSens3;
    private Rigidbody rbUS3;
    public UltrasonicMeassure ultraSens4;
    private Rigidbody rbUS4;
    public UltrasonicMeassure ultraSens5;
    private Rigidbody rbUS5;

    private float visibleDistance = 50f;
    private float US1distance = 0f;
    private float US2distance = 0f;
    private float US3distance = 0f;
    private float US4distance = 0f;
    private float US5distance = 0f;

    public float US1deltaDistance = 1f;
    public float US2deltaDistance = 1f;
    public float US3deltaDistance = 1f;
    public float US4deltaDistance = 1f;
    public float US5deltaDistance = 1f;

    public float US1distanceOld = 0f;
    public float US2distanceOld = 0f;
    public float US3distanceOld = 0f;
    public float US4distanceOld = 0f;
    public float US5distanceOld = 0f;

    /// Phototransistor sensors input 
    public PhototransistorMeassure phototransistor1;
    public PhototransistorMeassure phototransistor2;
    public PhototransistorMeassure phototransistor3;
    public PhototransistorMeassure phototransistor4;

    List<float> US1dist = new List<float>();
    List<float> US2dist = new List<float>();
    List<float> US3dist = new List<float>();
    List<float> US4dist = new List<float>();
    List<float> US5dist = new List<float>();

    /// Velocity input
    private float velMax = 39f;
    List<float> velX = new List<float>();
    List<float> velZ = new List<float>();

    /// Variables for saving vector observation:
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

    /// Light intensity input:
    private float intesityMax = 120f;
    private Vector2 intens1_vect;
    private Vector2 intens2_vect;
    private Vector2 intens3_vect;
    private Vector2 intens4_vect;

    private float intensity1;
    private float intensity2;
    private float intensity3;
    private float intensity4;

    private bool startRes = true;

    public override void InitializeAgent()
    {
        base.InitializeAgent();

        /// initialise delta intensity variables:
        intensity = intensity1 + intensity2 + intensity3 + intensity4;
        intensityOld = 0.0f;

        /// initialise reset position variables:
        rb = this.GetComponent<Rigidbody>();
        rbUS1 = ultraSens1.GetComponent<Rigidbody>();
        rbUS2 = ultraSens2.GetComponent<Rigidbody>();
        rbUS3 = ultraSens3.GetComponent<Rigidbody>();
        rbUS4 = ultraSens4.GetComponent<Rigidbody>();
        rbUS5 = ultraSens5.GetComponent<Rigidbody>();
        vehicleStartRot = this.transform.rotation;
        vehicleStartPos = this.transform.position;
        lightStartPos = LightSource.transform.position;
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
        AddVectorObs(US1distance);
        AddVectorObs(US2distance);
        AddVectorObs(US3distance);
        AddVectorObs(US4distance);
        AddVectorObs(US5distance);

        AddVectorObs(Mathf.Clamp((rb.velocity.x / velMax), 0, 1));
        AddVectorObs(Mathf.Clamp((rb.velocity.z / velMax), 0, 1));
        
        AddVectorObs(intensity1);
        AddVectorObs(intensity2);
        AddVectorObs(intensity3);
        AddVectorObs(intensity4);

        /// for saving vector observation:
        US1dist.Add(US1distance);
        US2dist.Add(US2distance);
        US3dist.Add(US3distance);
        US4dist.Add(US4distance);
        US5dist.Add(US5distance);
        velX.Add(rb.velocity.x / velMax);
        velZ.Add(rb.velocity.z / velMax);
        intens1.Add(intensity1);
        intens2.Add(intensity2);
        intens3.Add(intensity3);
        intens4.Add(intensity4);
    }

    /// Moving agent with outputs:
    public override void AgentAction(float[] act, string textAction)
    {
        base.AgentAction(act, textAction);

        /// Ultrasonic sensors distance counting:
        US1distance = 0f;
        US2distance = 0f;
        US3distance = 0f;
        US4distance = 0f;
        US5distance = 0f;

        int layerMask = 1 << 11;
        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, this.transform.forward * hit.distance, Color.red);
            US1distance = Mathf.Clamp(hit.distance / visibleDistance, 0, 1);
        }

        if (Physics.Raycast(this.transform.position, this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, this.transform.right * hit.distance, Color.red);
            US2distance = Mathf.Clamp(hit.distance / visibleDistance, 0, 1);
        }
        
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right * hit.distance, Color.red);
            US3distance = Mathf.Clamp(hit.distance / visibleDistance, 0, 1);
        }
        
        if (Physics.Raycast(this.transform.position, -this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, -this.transform.right * hit.distance, Color.red);
            US4distance = Mathf.Clamp(hit.distance / visibleDistance, 0, 1);
        }
        
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right * hit.distance, Color.red);
            US5distance = Mathf.Clamp(hit.distance / visibleDistance, 0, 1);
        }
        
        /// light intensity counting:
        intens1_vect.Set((LightSource.transform.position.x - phototransistor1.transform.position.x), (LightSource.transform.position.z - phototransistor1.transform.position.z));
        intens2_vect.Set((LightSource.transform.position.x - phototransistor2.transform.position.x), (LightSource.transform.position.z - phototransistor2.transform.position.z));
        intens3_vect.Set((LightSource.transform.position.x - phototransistor3.transform.position.x), (LightSource.transform.position.z - phototransistor3.transform.position.z));
        intens4_vect.Set((LightSource.transform.position.x - phototransistor4.transform.position.x), (LightSource.transform.position.z - phototransistor4.transform.position.z));

        intensity1 = Mathf.Clamp((1 - intens1_vect.magnitude / intesityMax), 0, 1);
        intensity2 = Mathf.Clamp((1 - intens2_vect.magnitude / intesityMax), 0, 1);
        intensity3 = Mathf.Clamp((1 - intens3_vect.magnitude / intesityMax), 0, 1);
        intensity4 = Mathf.Clamp((1 - intens4_vect.magnitude / intesityMax), 0, 1);

        intensity = intensity1 + intensity2 + intensity3 + intensity4;
                
        
        /// counting delta intensity and use it to punish or reward:
        deltaCounter--;
        if (deltaCounter == 0)
        {
            deltaCounter = 1;
            deltaIntensity = intensity - intensityOld;
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
        
        //AddReward(-0.001f);
        /// for delta distance input: 
        if (intensity > 3.5f)
        {
            SetReward(1f);
            Done();
        }

        ///add position and rotation:
        //rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 80f);
        //rb.AddTorque(transform.up * Mathf.Clamp(act[1], -1f, 1f) * 300f);

        rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 300f);

        //this.transform.position += this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 0.5f;
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
        LightSource.transform.position = lightStartPos + new Vector3(Random.Range(-20, 20), 0, (Random.Range(-20, 10)));
        wall.transform.position = wallStartPos + new Vector3(Random.Range(-10, 10), 0, 0);
        wall.transform.Rotate(0f, Random.Range(-180, 180), 0f, 0);
        wall.transform.localScale = new Vector3(Random.Range(10, 20), 30, 4);
        wall2.transform.position = wall2StartPos + new Vector3(Random.Range(-10, 10), 0, 0);
        wall2.transform.Rotate(0f, Random.Range(-180, 180), 0f, 0);
        wall2.transform.localScale = new Vector3(Random.Range(10, 20), 30, 4);
        deltaCounter = 20;
        intensityOld = 0.0f;
        /*
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
        */

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
    }

    float Round(float x)
    {
        return (float)System.Math.Round(x, 5, System.MidpointRounding.AwayFromZero);
        //(float)System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 10.0f;
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
        /*
        GUI.color = Color.red;
        GUI.Label(new Rect(25, 25, 250, 30), "US1: " + Round(US1distance));
        GUI.Label(new Rect(25, 50, 250, 30), "US2: " + Round(US2distance));
        GUI.Label(new Rect(25, 75, 250, 30), "US3: " + Round(US3distance));
        GUI.Label(new Rect(25, 100, 250, 30), "US4: " + Round(US4distance));
        GUI.Label(new Rect(25, 125, 250, 30), "US5: " + Round(US5distance));
        
        GUI.color = Color.yellow;
        GUI.Label(new Rect(300, 25, 250, 30), "US1 velocity: " + Mathf.Clamp(Round(rb.velocity.x / velMax), 0, 1));
        GUI.Label(new Rect(300, 50, 250, 30), "US2 velocity: " + Mathf.Clamp(Round(rb.velocity.z / velMax), 0, 1));
        
        GUI.color = Color.blue;
        GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + Round(intensity1));
        GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + Round(intensity2));
        GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + Round(intensity3));
        GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + Round(intensity4));

        GUI.color = Color.green;
        GUI.Label(new Rect(500, 25, 250, 30), "delta intensity: " + deltaIntensity);
        GUI.Label(new Rect(500, 50, 250, 30), "reward: " + showReward);
        GUI.Label(new Rect(500, 75, 250, 30), "Intensity: " + intensity);
        */
    }
}
