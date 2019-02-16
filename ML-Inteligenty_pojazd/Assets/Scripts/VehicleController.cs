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
    public GameObject LightSource;
    public float motorForce = 50.0f;
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
    private bool win = false;
    private double intensityOld;
    private double intensityNew;
    private double deltaIntensity;

    private ANN ann;
    private List<double> calcOutputs;
    private List<double> states;
    private List<double> qs;
    //private Vector3 vehicleStartPos;
    private Quaternion vehicleStartRot;

    private float reward = 0.0f;                            //reward to associate with actions
    private float rewardSum = 0.0f;
    private float punishSum = 0.0f;
    private List<Replay> replayMemory = new List<Replay>(); //memory - list of past actions and rewards
    private int mCapacity = 10000;                          //memory capacity

    private float discount = 0.99f;                         //how much future states affect rewards
    private float exploreRate = 100.0f;                     //chance of picking random action
    private float maxExploreRate = 100.0f;					//max chance value
    private float minExploreRate = 0.01f;					//min chance value
    private float exploreDecay = 0.01f;
    private int resetTimer;
    private int lightTimer;
    
    // For OnGUI to display
    private int failCount = 0;
    private float timer = 0;
    private float bestTime = 0;
    
    private void Start()
    {
        ann = new ANN(9, 4, 2, 18, 0.2f);
        //vehicleStartPos = this.transform.position;
        vehicleStartRot = this.transform.rotation;
        intensityOld = 0;
        resetTimer = 100;
        lightTimer = 50;
        Time.timeScale = 40.0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
            ResetVehicle();
        if (this.transform.rotation.z < -0.12f || this.transform.rotation.z > 0.12f)
        {
            ResetVehicle();
        }
        if (Input.GetKeyDown("0"))
            Time.timeScale = 1f;
        if (Input.GetKeyDown("1"))
            Time.timeScale = 10f;
        if (Input.GetKeyDown("2"))
            Time.timeScale = 20f;
        if (Input.GetKeyDown("3"))
            Time.timeScale = 30f;
        if (Input.GetKeyDown("4"))
            Time.timeScale = 40f;
        if (Input.GetKeyDown("5"))
            Time.timeScale = 100f;
        if (Input.GetKeyDown("6"))
            Time.timeScale = 150f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            collisionFail = true;
        }

        if (collision.gameObject.tag == "Win")
        {
            win = true;
        }
    }

    void ResetVehicle()
    {
        
        this.transform.position = new Vector3(Random.Range(-580, 580), 4, (Random.Range(-230, 880))); 
        this.transform.rotation = vehicleStartRot;
        LightSource.transform.position = new Vector3(Random.Range(-580, 580), 404, (Random.Range(-230, 880)));
    }
    
    private void LightFail(double deltaIntens)
    {
        if (deltaIntens <= 0.00f)
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
        intensityNew = (phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity) / 4;

        //intensityNew = 1.37f - (phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity) /4;
        deltaIntensity = intensityNew - intensityOld;
        //deltaIntensity =  intensityOld - intensityNew;
        LightFail(deltaIntensity);

        resetTimer--;
        timer += Time.deltaTime;
        states = new List<double>();
        qs = new List<double>();

        states.Add(ultraSens1.distance);
        states.Add(ultraSens2.distance);
        states.Add(ultraSens3.distance);
        states.Add(ultraSens4.distance);
        states.Add(ultraSens5.distance);
        
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
            rightWheel_C.motorTorque = (float)qs[maxQIndex] * motorForce;// * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexRight: " + (float)qs[maxQIndex]);
        } 

        if (maxQIndex == 1)
        {
            leftWheel_C.motorTorque = (float)qs[maxQIndex] * motorForce;// * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexLeft: " + (float)qs[maxQIndex]);
        }

        if (maxQIndex == 2)
        {
            rightWheel_C.motorTorque = (float)qs[maxQIndex] * -motorForce;// * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexBackRight: " + (float)qs[maxQIndex]);
        }

        if (maxQIndex == 3)
        {
            leftWheel_C.motorTorque = (float)qs[maxQIndex] * -motorForce;// * Time.deltaTime; // * verticalInput;
            //Debug.Log(timer + "maxQIndexBackLeft: " + (float)qs[maxQIndex]);
        }
        //Debug.Log("1: " + (float)qs[maxQIndex]);

        if (collisionFail)
        {
            reward = -1.0f;
            punishSum += reward;
        }
        else
        {
            reward = 0.1f;
            rewardSum += reward;
        }
        if (lightFail)
        {
            reward += -0.1f;
            punishSum += reward;
            //lightFail = false;            
        }
        else
        {
            reward += 0.1f;
            rewardSum += reward;
        }
        if (win)
        {
            reward += 1f;
        }
        reward += -0.005f;

        Replay lastMemory = new Replay(states, reward);

        if (replayMemory.Count > mCapacity)
            replayMemory.RemoveAt(0);

        replayMemory.Add(lastMemory);

        if (collisionFail || resetTimer == 0 || win)
        {
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
                    feedback = replayMemory[i].reward;             
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

            win = false;
            timer = 0;
            ResetVehicle();
            collisionFail = false;
            resetTimer = 100;
            replayMemory.Clear();
            failCount++;
        }
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
        
        if (lightTimer == 50)
            intensityOld = (phototransistor1.intensity + phototransistor2.intensity + phototransistor3.intensity + phototransistor4.intensity) / 4;

        lightTimer--;

        if (lightTimer == 0)
        {
            lightTimer = 50;
            Drive();
        }
        
        Steer();
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
        GUI.Label(new Rect(500, 150, 250, 30), "Time Scale: " + Time.timeScale);
        GUI.Label(new Rect(500, 175, 250, 30), "Light Fail: " + lightFail);
        GUI.Label(new Rect(500, 200, 250, 30), "Delta light: " + deltaIntensity);
        GUI.Label(new Rect(500, 225, 250, 30), "Reward Sum: " + rewardSum);
        GUI.Label(new Rect(500, 250, 250, 30), "Punishment Sum: " + punishSum);
        GUI.Label(new Rect(500, 275, 250, 30), "Intensity Old: " + intensityOld);
        GUI.Label(new Rect(500, 300, 250, 30), "Intensity New: " + intensityNew);
    }

}
