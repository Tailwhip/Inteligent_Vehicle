using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Replay
{
    public List<double> states;
    public double reward;

    public Replay(List<double> inputs, double r)
    {
        states = new List<double>();
        for (int i = 0; i < inputs.Count ; i++)
        {
            states.Add(inputs[i]);
        }
        reward = r;
    }
}

public class VehicleController : MonoBehaviour {

    // Wheels input
    public WheelCollider rightWheel_C, leftWheel_C, backWheel_C;
    public Transform rightWheel_T, leftWheel_T, backWheel_T;
    public float motorForce = 500.0f;
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

    private bool collisionFail = false;
    private bool lightFail = false;
    private double intensityOld;
    private double intensityNew;
    private double deltaIntensity;

    private ANN ann;
    private List<double> calcOutputs;
    private List<double> states;
    private List<double> qs;
    private Vector3 vehicleStartPos;
    private Quaternion vehicleStartRot;

    private float reward = 0.0f;                            //reward to associate with actions
    private List<Replay> replayMemory = new List<Replay>(); //memory - list of past actions and rewards
    private int mCapacity = 20000;                          //memory capacity

    private float discount = 0.99f;                         //how much future states affect rewards
    private float exploreRate = 100.0f;                     //chance of picking random action
    private float maxExploreRate = 100.0f;					//max chance value
    private float minExploreRate = 0.01f;					//min chance value
    private float exploreDecay = 0.001f;
    private int homeworkTimer;
    private int lightTimer;
    
    // For OnGUI to display
    private int failCount = 0;
    private float timer = 0;
    private float bestTime = 0;
    
    private void Start()
    {
        ann = new ANN(4, 4, 2, 8, 0.3f);
        vehicleStartPos = this.transform.position;
        vehicleStartRot = this.transform.rotation;
        intensityOld = 0;
        Time.timeScale = 20.0f;
        homeworkTimer = 10000;
        lightTimer = 1000;
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
            ResetVehicle();
        if (this.transform.rotation.z < -0.12f || this.transform.rotation.z > 0.12f)
        {
            ResetVehicle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            collisionFail = true;
        }
    }

    void ResetVehicle()
    {
        this.transform.position = vehicleStartPos;
        this.transform.rotation = vehicleStartRot;
    }
    
    private void LightFail(double deltaIntens)
    {
        if (deltaIntens <= 0.05f)
            lightFail = true;
    }
      

    private void Steer()
    {
        if(Input.GetKeyDown("q"))
        {
            rightWheel_C.motorTorque = motorForce * Time.deltaTime;
        }

        if (Input.GetKeyDown("e"))
        {
            leftWheel_C.motorTorque = motorForce * Time.deltaTime;
        }

        if (Input.GetKeyDown("a"))
        {
            rightWheel_C.motorTorque = -motorForce * Time.deltaTime;
        }

        if (Input.GetKeyDown("d"))
        {
            leftWheel_C.motorTorque = -motorForce * Time.deltaTime;
        }
    }

    private void Drive()
    {
        lightTimer--;
        if (lightTimer == 0)
        {
            intensityOld = intensityNew;
            lightTimer = 1000;
        }
        
        intensityNew = (phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity);
        deltaIntensity = intensityNew - intensityOld;
        LightFail(deltaIntensity);
        //intensityOld = intensityNew;
        //Debug.Log("LightFail: " + lightFail);
        //Debug.Log("IntensityNew: " + intensityNew + " IntensityOld: " + intensityOld);
        //Debug.Log("DeltaIntensity: " + deltaIntensity);

        homeworkTimer--;
        timer += Time.deltaTime;
        states = new List<double>();
        qs = new List<double>();

        /*
        states.Add(ultraSens1.distance);
        states.Add(ultraSens2.distance);
        states.Add(ultraSens3.distance);
        states.Add(ultraSens4.distance);
        states.Add(ultraSens5.distance);
        */
        states.Add(phototransistor1.intensity);
        states.Add(phototransistor2.intensity);
        states.Add(phototransistor3.intensity);
        states.Add(phototransistor4.intensity);

        qs = SoftMax(ann.CalcOutput(states));
        double maxQ = qs.Max();
        int maxQIndex = qs.ToList().IndexOf(maxQ);
        //Debug.Log(timer + "maxQ: " + maxQ);
        exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);
        
        if(Random.Range(0, 100) < exploreRate)
            maxQIndex = Random.Range(0, 4);
        
        if (maxQIndex == 0)
        {
            rightWheel_C.motorTorque = (float)qs[maxQIndex] * motorForce * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexRight: " + (float)qs[maxQIndex]);
        } 

        if (maxQIndex == 1)
        {
            leftWheel_C.motorTorque = (float)qs[maxQIndex] * motorForce * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexLeft: " + (float)qs[maxQIndex]);
        }

        if (maxQIndex == 2)
        {
            rightWheel_C.motorTorque = (float)qs[maxQIndex] * -motorForce * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexBackRight: " + (float)qs[maxQIndex]);
        }

        if (maxQIndex == 3)
        {
            leftWheel_C.motorTorque = (float)qs[maxQIndex] * -motorForce * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexBackLeft: " + (float)qs[maxQIndex]);
        }

        //Debug.Log(timer + "All values: " + (float)qs[0] + "; " + (float)qs[1] + "; " + (float)qs[2] + "; " + (float)qs[3]);
        /*
        if (collisionFail)
            reward = -1.0f;
        else
            reward = 0.2f;
            */
        if (lightFail)
        {
            reward = -1.0f;
            lightFail = false;
            
        }
        else
        {
            reward = (float)intensityNew / 4;
        }

        //Debug.Log("Reward: " + reward);

        //Debug.Log(timer + " Reward: " + reward);
        /*
        if (lightFail)
        {
            //Debug.Log(timer + "LightFail: " + lightFail);
            reward += -(float)intensityNew * 10.0f;
            //lightFail = false;
        }
        
        if (!lightFail)
        {
            //Debug.Log(timer + "LightFail: " + lightFail);
            reward += (float)intensityNew;
        }

        Debug.Log("Reward: " + reward);
        */

        Replay lastMemory = new Replay(states, reward);

        if (replayMemory.Count > mCapacity)
            replayMemory.RemoveAt(0);

        replayMemory.Add(lastMemory);

        if (collisionFail)
        {
            ResetVehicle();
            collisionFail = false;
        }

        if (homeworkTimer == 0)
        {
            int j = 0;
            for (int i = replayMemory.Count - 1; i >= 0; i--)
            {
                
                List<double> toutputsOld = new List<double>();
                List<double> toutputsNew = new List<double>();
                toutputsOld = SoftMax(ann.CalcOutput(replayMemory[i].states));

                double maxQOld = toutputsOld.Max();
                int action = toutputsOld.ToList().IndexOf(maxQOld);

                double feedback;
                if (i == replayMemory.Count - 1 || replayMemory[i].reward < 0)
                {
                    j++;
                    feedback = replayMemory[i].reward;
                    //Debug.Log("ZNACZNIK " + j);
                }
                    
                else
                {
                    toutputsNew = SoftMax(ann.CalcOutput(replayMemory[i + 1].states));
                    maxQ = toutputsNew.Max();
                    feedback = (replayMemory[i].reward +
                        discount * maxQ);
                }

                toutputsOld[action] = feedback;
                ann.Train(replayMemory[i].states, toutputsOld);
            }

            if (timer > bestTime)
            {
                bestTime = timer;
            }

            timer = 0;
            /*
            if (collisionFail)
                ResetVehicle();
                */
            //collisionFail = false;
            homeworkTimer = 10000;
            //lightFail = false;
            replayMemory.Clear();
            failCount++;
        }
        //Debug.Log("ReplayMemoryCount: " + replayMemory.Count);
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
        transform.position = pos; //+ diverse;
        transform.rotation = quat;
    }

    private void FixedUpdate()
    {
        //Steer();
        Drive();
        UpdateWheelPoses();
    }

    List<double> SoftMax(List<double> values)
    {
        double max = values.Max();

        float scale = 0.0f;
        for (int i = 0; i < values.Count; ++i)
            scale += Mathf.Exp((float)(values[i] - max));

        List<double> result = new List<double>();
        for (int i = 0; i < values.Count; ++i)
            result.Add(Mathf.Exp((float)(values[i] - max)) / scale);

        return result;
    }

    float Map(float newfrom, float newto, float origfrom, float origto, float value)
    {
        if (value <= origfrom)
            return newfrom;
        else if (value >= origto)
            return newto;
        return (newto - newfrom) * ((value - origfrom) / (origto - origfrom)) + newfrom;
    }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(25, 25, 250, 30), "US1: " + ultraSens1.distance);
        GUI.Label(new Rect(25, 50, 250, 30), "US2: " + ultraSens2.distance);
        GUI.Label(new Rect(25, 75, 250, 30), "US3: " + ultraSens3.distance);
        GUI.Label(new Rect(25, 100, 250, 30), "US4: " + ultraSens4.distance);
        GUI.Label(new Rect(25, 125, 250, 30), "US5: " + ultraSens5.distance);
        GUI.color = Color.blue;
        GUI.Label(new Rect(200, 25, 250, 30), "PT1: " + phototransistor1.intensity);
        GUI.Label(new Rect(200, 50, 250, 30), "PT2: " + phototransistor2.intensity);
        GUI.Label(new Rect(200, 75, 250, 30), "PT3: " + phototransistor3.intensity);
        GUI.Label(new Rect(200, 100, 250, 30), "PT4: " + phototransistor4.intensity);
        GUI.color = Color.green;
        GUI.Label(new Rect(500, 25, 250, 30), "Fails: " + failCount);
        GUI.Label(new Rect(500, 50, 250, 30), "Decay Rate: " + exploreRate);
        GUI.Label(new Rect(500, 75, 250, 30), "Best Time: " + bestTime);
        GUI.Label(new Rect(500, 100, 250, 30), "Timer: " + timer);
        GUI.Label(new Rect(500, 125, 250, 30), "Reward: " + reward);
    }







}
