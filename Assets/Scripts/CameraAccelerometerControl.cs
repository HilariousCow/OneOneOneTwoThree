using UnityEngine;
using System.Collections;

public class CameraAccelerometerControl : MonoBehaviour {

	// Use this for initialization
    public float SmoothAmount = 300f;

    private Quaternion startRotation;
    private Quaternion startGyro;
	void Start ()
	{
	    startRotation = Quaternion.Inverse(transform.rotation);
	    Input.gyro.enabled = true;

	    ResetGyro();
	    startGyro = ConvertRotation(Quaternion.Inverse(Input.gyro.attitude));
	}

    private void ResetGyro()
    {
        startGyro = Quaternion.Inverse(Input.gyro.attitude);
    }
	
	// Update is called once per frame
	void Update ()
	{
        if(Input.GetMouseButtonDown(0))
        {
            ResetGyro();
        }


        //almost there. just a bit of a mind fuck i guess
        Quaternion rot = ConvertRotation(Input.gyro.attitude);

       // rot = Quaternion.Euler(rot.eulerAngles.x, -rot.eulerAngles.y, rot.eulerAngles.z);

    //    transform.position = startRotation * startGyro * rot * Vector3.forward * -transform.position.magnitude;
        //transform.rotation = Quaternion.LookRotation(-transform.position.normalized, startRotation*rot * Vector3.up);
        rot = ConvertRotation(rot);
	    rot = Quaternion.Euler(-rot.eulerAngles.x, -rot.eulerAngles.y, rot.eulerAngles.z);

        rot = Quaternion.AngleAxis(90f, Vector3.right) * rot;

	    float angle = Quaternion.Angle(rot, transform.rotation);
	    angle /= 180.0f;



        transform.rotation = Quaternion.Lerp(transform.rotation, rot, angle * Time.deltaTime * SmoothAmount);

	}

    private static Quaternion ConvertRotation(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

     
    void OnGUI()
    {
        GUILayout.Label("Gyro: " + Input.gyro.attitude.ToString());
        GUILayout.Label("Gyro Euler: " + Input.gyro.attitude.eulerAngles.ToString());
    }
}
