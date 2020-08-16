using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driving : MonoBehaviour
{
    GameObject FrontLeftWheel;
    GameObject FrontRightWheel;
    GameObject RearLeftWheel;
    GameObject RearRightWheel;
    // Start is called before the first frame update
    void Start()
    {
        FrontLeftWheel = GameObject.Find("FrontLeft");
        FrontRightWheel = GameObject.Find("FrontRight");
        RearLeftWheel = GameObject.Find("RearLeft");
        RearRightWheel = GameObject.Find("RearRight");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCarSpeed(string speedString)
    {
        int speed = int.Parse(speedString);

        FrontLeftWheel.GetComponent<Animator>().SetInteger("speed", speed);
        FrontRightWheel.GetComponent<Animator>().SetInteger("speed", speed);
        RearLeftWheel.GetComponent<Animator>().SetInteger("speed", speed);
        RearRightWheel.GetComponent<Animator>().SetInteger("speed", speed);
    }
}
