using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombunnyDecision : Decision
{

    public override float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        /*
        if (brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            List<float> act = new List<float>();

            for (int i = 0; i < brainParameters.vectorActionSize[0]; i++)
            {
                act.Add(2 * Random.value - 1);
            }

            return act.ToArray();
        }
        else
        {
            float[] act = new float[brainParameters.vectorActionSize.Length];
            for (int i = 0; i < brainParameters.vectorActionSize.Length; i++)
            {
                act[i] = Random.Range(0, brainParameters.vectorActionSize[i]);
            }
            return act;
        }
        */

        float[] action = new float[2];
        action[0] = Random.Range(0.0f, 1.0f);
        action[1] = Random.Range(-1.0f, 1.0f);
        return action;
    }

    public override List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        return new List<float>();

    }

}
