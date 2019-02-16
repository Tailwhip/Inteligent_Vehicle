using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombunnyAgent : Agent
{
    public GameObject goal;
    public GameObject area;
    Rigidbody rb;
    Vector3 startPosition;
    Vector3 goalPos;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rb = this.GetComponent<Rigidbody>();
        startPosition = this.transform.position;
        goalPos = goal.transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "goal")
        {
            SetReward(1f);
            Done();
            AgentReset();
        }

        else if (collision.gameObject.tag == "dead")
        {
            SetReward(-1f);
            Done();
            AgentReset();
        }
    }

    public override void CollectObservations()
    {
        base.CollectObservations();
        AddVectorObs((this.transform.position.x - area.transform.position.x));
        AddVectorObs((this.transform.position.z - area.transform.position.z));
        AddVectorObs((goal.transform.position.x - area.transform.position.x));
        AddVectorObs((goal.transform.position.z - area.transform.position.z));
        AddVectorObs(rb.velocity.x);
        AddVectorObs(rb.velocity.z);
    }

    public override void AgentAction(float[] act, string textAction)
    {
        base.AgentAction(act, textAction);
        AddReward(-0.005f);
        //original solution:
        /*
        Vector3 dir;
        dir.x = Mathf.Clamp(act[0], -1f, 1f);
        dir.z = Mathf.Clamp(act[1], -1f, 1f);
        rb.AddForce(new Vector3(dir.x * 40.0f, 0, dir.z * 40.0f));
        */
        /*
        //forces
        rb.AddForce(this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 20f);
        rb.AddTorque(new Vector3(0, Mathf.Clamp(act[1], -1f, 1f) * 200f, 0));
        */

        //add position and rotation:
        this.transform.position += this.transform.forward * Mathf.Clamp(act[0], -1f, 1f) * 0.1f;
        this.transform.position += this.transform.right * Mathf.Clamp(act[1], -1f, 1f) * 0.1f;
        //this.transform.Rotate(0, Mathf.Clamp(act[1], -1f, 1f), 0, 0);
    }


    public override void AgentReset()
    {
        base.AgentReset();
        this.transform.position = startPosition + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
        rb.velocity = Vector3.zero;
        goal.transform.position = goalPos + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
    }

}
