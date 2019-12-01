using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PhototransistorMeassure : MonoBehaviour
{
    public float intensity;
    //private float intesityMax = 120f;
    private Vector2 intensVect = new Vector2(0f, 0f);
    public GameObject lightSource;
    private float distance;
    private float angleXY;
    private float angleYZ;


    void FixedUpdate()
    {
        /// light intensity counting:
        intensVect.Set((lightSource.transform.position.x - this.transform.position.x), (lightSource.transform.position.z - this.transform.position.z));
        //intensity = Mathf.Clamp(1 - (intensVect.magnitude / intesityMax), 0, 1);

        distance = intensVect.magnitude;
        //Debug.Log("distance: " + distance);

        angleXY = Mathf.Atan((lightSource.transform.position.x - this.transform.position.x) / lightSource.transform.position.y) * Mathf.Rad2Deg;
        angleYZ = Mathf.Atan((lightSource.transform.position.y - this.transform.position.y) / lightSource.transform.position.y) * Mathf.Rad2Deg;
                 
        /// intensity is a real BH1750 sensor value obtained from sensor characteristic multiplied 
        /// by ratio of angle deviation from an axis given from the datasheet
        intensity = Round(Mathf.Clamp((Mathf.Log((axisValue(distance) * angleRatio(angleXY, angleYZ)), 110f) + 1.178796054f), 0f, 1f));
                
        //intensity = Mathf.Clamp(1 - (intensVect.magnitude / intesityMax), 0, 1);
    }

    float axisValue(float _distance)
    {
        if (_distance > 450)
        {
            //Debug.Log("Wychodzi: " + (Mathf.Log(Mathf.Pow(0.993f, _distance + 120f) + 0.00354f, 110f) + 1.178796054f));
            //Debug.Log("Czujnik: " + (Mathf.Pow(0.993f, _distance + 120) + 0.00354f));
            return Mathf.Clamp((Mathf.Pow(0.995f, _distance + 390f) + 0.003f), 0, 0.76f);
            //Mathf.Pow(0.993f, _distance + 120) + 0.00354f;
        }

        else
        {
            //Debug.Log("Wychodzi: " + (Mathf.Log((Mathf.Pow(0.981f, _distance - 11f)) + 0.085f, 110f) + 1.178796054f));
            //Debug.Log("Czujnik: " + Mathf.Pow(0.981f, _distance - 11) + 0.085f);
            return Mathf.Clamp((Mathf.Pow(0.993f, _distance + 150f) + 0.003f), 0, 0.76f);
            //Mathf.Pow(0.981f, _distance - 11) + 0.085f;
        }
    }

    float angleRatio(float _angleXY, float _angleYZ)
    {
        float x1 = (Mathf.Pow(_angleXY, 2f) * (-0.000123f) + 1f);
        /// both of ratio of angle deviation from an axis given in the datasheet 
        /// are interpolated by one simple square function but in case it's not enough
        /// it's gonna have to be replaced by more acurrate iterpolations of both deviation plots
        //float x2 = (Mathf.Pow(_angleXY, 2f) * (-0.000123f) + 1f);
        //return (x1 + x2) / 2;
        return x1;
    }

    float Round(float x)
    {
        return (float)System.Math.Round(x, 2);
    }
}
