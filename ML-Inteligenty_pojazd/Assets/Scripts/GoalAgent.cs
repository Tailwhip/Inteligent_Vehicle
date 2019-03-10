using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    /// variables to count delta intensity:
    private float deltaIntensity;
    private float intensityOld = 0f;
    private float intensity = 0f;
    private int deltaCounter = 1;

    /// light intensity input:
    private Vector2 intensity1;
    private Vector2 intensity2;
    private Vector2 intensity3;
    private Vector2 intensity4;

    private bool startRes = true;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
           
        /// initialise delta intensity variables:
        intensity = (1f - (intensity1.magnitude / 100f)) + (1f - (intensity2.magnitude / 100f)) + (1f - (intensity3.magnitude / 100f)) + (1f - (intensity4.magnitude / 100f));
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
        /*
        AddVectorObs(ultraSens1.distance);
        AddVectorObs(ultraSens2.distance);
        AddVectorObs(ultraSens3.distance);
        AddVectorObs(ultraSens4.distance);
        AddVectorObs(ultraSens5.distance);
        */
        
        AddVectorObs(rb.velocity.x);
        AddVectorObs(rb.velocity.z);
        
        /*
        AddVectorObs(US1deltaDistance);
        AddVectorObs(US2deltaDistance);
        AddVectorObs(US3deltaDistance);
        AddVectorObs(US4deltaDistance);
        AddVectorObs(US5deltaDistance);
        /*
        AddVectorObs(ultraSens1.deltaDistance);
        AddVectorObs(ultraSens2.deltaDistance);
        AddVectorObs(ultraSens3.deltaDistance);
        AddVectorObs(ultraSens4.deltaDistance);
        AddVectorObs(ultraSens5.deltaDistance);
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

    /// Moving agent with outputs:
    public override void AgentAction(float[] act, string textAction)
    {
        base.AgentAction(act, textAction);

        /// Ultrasonic sensors distance counting:
        
        int layerMask = 1 << 11;
        RaycastHit hit;

        //Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right * visibleDistance, Color.red);

        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, this.transform.forward * hit.distance, Color.red);
            //dist = 1 - hit.distance / visibleDistance;
            US1distance = hit.distance;
        }

        if (Physics.Raycast(this.transform.position, this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, this.transform.right * hit.distance, Color.red);
            //dist = 1 - hit.distance / visibleDistance;
            US2distance = hit.distance;
        }
        
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right * hit.distance, Color.red);
            //dist = 1 - hit.distance / visibleDistance;
            US3distance = hit.distance;
        }
        
        if (Physics.Raycast(this.transform.position, -this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, -this.transform.right * hit.distance, Color.red);
            //dist = 1 - hit.distance / visibleDistance;
            US4distance = hit.distance;
        }
        
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, out hit, visibleDistance, layerMask))
        {
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right * hit.distance, Color.red);
            //dist = 1 - hit.distance / visibleDistance;
            US5distance = hit.distance;
        }
        
        /// light intensity counting:
        intensity1.Set((LightSource.transform.position.x - phototransistor1.transform.position.x), (LightSource.transform.position.z - phototransistor1.transform.position.z));
        intensity2.Set((LightSource.transform.position.x - phototransistor2.transform.position.x), (LightSource.transform.position.z - phototransistor2.transform.position.z));
        intensity3.Set((LightSource.transform.position.x - phototransistor3.transform.position.x), (LightSource.transform.position.z - phototransistor3.transform.position.z));
        intensity4.Set((LightSource.transform.position.x - phototransistor4.transform.position.x), (LightSource.transform.position.z - phototransistor4.transform.position.z));

        intensity = (1f - (intensity1.magnitude / 100f)) + (1f - (intensity2.magnitude / 100f)) + (1f - (intensity3.magnitude / 100f)) + (1f - (intensity4.magnitude / 100f));

        /// counting delta intensity and use it to punish or reward:
        deltaCounter--;
        if (deltaCounter == 0)
        {
            deltaCounter = 1;
            
            deltaIntensity = intensity - intensityOld;
            if (deltaIntensity <= 0.00f)
            {
                AddReward(-0.005f);
                //Debug.Log("LOOSE1");
                looseCount++;
            }
            
            intensityOld = intensity;
            /*
            US1deltaDistance =  US1distanceOld - US1distance;
            US1distanceOld = US1distance;

            US2deltaDistance =  US2distanceOld - US2distance;
            US2distanceOld = US2distance;

            US3deltaDistance = US3distanceOld - US3distance;
            US3distanceOld = US3distance;

            US4deltaDistance = US4distanceOld - US4distance;
            US4distanceOld = US4distance;

            US5deltaDistance = US5distanceOld - US5distance;
            US5distanceOld = US5distance;
            */
        }
        
        //AddReward(-0.003f);

        /// for delta distance input: 
        if (intensity > 3.0f)
        {
            SetReward(1f);
            Done();
        }

        /*
        /// for PTs input:
        if (intensity < 80f)
        {
            SetReward(1f);
            Done();
        }
        */
        ///add position and rotation:

        //rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 100f);

        this.transform.position += this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 0.5f;
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
    }

    // Showing ANN data:
    private void OnGUI()
    {
        /*
        GUI.color = Color.red;
                    
        GUI.Label(new Rect(25, 25, 250, 30), "US1: " + US1distance);
        GUI.Label(new Rect(25, 50, 250, 30), "US2: " + US2distance);
        GUI.Label(new Rect(25, 75, 250, 30), "US3: " + US3distance);
        GUI.Label(new Rect(25, 100, 250, 30), "US4: " + US4distance);
        GUI.Label(new Rect(25, 125, 250, 30), "US5: " + US5distance);

        GUI.color = Color.yellow;
        GUI.Label(new Rect(300, 25, 250, 30), "US1 velocity: " + rb.velocity.x);
        GUI.Label(new Rect(300, 50, 250, 30), "US2 velocity: " + rb.velocity.z);

       GUI.color = Color.blue;
       GUI.Label(new Rect(150, 25, 250, 30), "PT1: " + (1f - (intensity1.magnitude / 100f)));
       GUI.Label(new Rect(150, 50, 250, 30), "PT2: " + (1f - (intensity2.magnitude / 100f)));
       GUI.Label(new Rect(150, 75, 250, 30), "PT3: " + (1f - (intensity3.magnitude / 100f)));
       GUI.Label(new Rect(150, 100, 250, 30), "PT4: " + (1f - (intensity4.magnitude / 100f)));

       GUI.color = Color.green;
       GUI.Label(new Rect(500, 25, 250, 30), "delta intensity: " + deltaIntensity);
       GUI.Label(new Rect(500, 50, 250, 30), "reward: " + showReward);
       GUI.Label(new Rect(500, 75, 250, 30), "Intensity: " + intensity);

       if (looseCount > 0)
       GUI.Label(new Rect(500, 150, 250, 30), "WIN/LOOSE: " + winCount / looseCount);
       */
    }
}
