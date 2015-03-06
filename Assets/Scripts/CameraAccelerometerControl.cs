using UnityEngine;
using System.Collections;

public class CameraAccelerometerControl : MonoBehaviour {

	// Use this for initialization
    public float SmoothAmount = 300f;
    public Renderer _webCamRenderer;


    private Quaternion startRotation;
	IEnumerator Start ()
	{
	    Input.gyro.enabled = true;
	    LockGyroYaw();
	    startRotation = Quaternion.AngleAxis(90f, Vector3.right);
        WebCamDevice[] devices = WebCamTexture.devices;

	    if (devices.Length > 0) {
		    WebCamTexture webCamTexture =new WebCamTexture(320, 240, 12);
            _webCamRenderer.material.mainTexture = webCamTexture;
            webCamTexture.Play();
	    } else {
		    Debug.LogError("No webcam devices found");
	    }

	    _webCamRenderer.enabled = false;

#if UNITY_WEBPLAYER
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam );
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
        }
        else
        {
        }
#else
	    yield return null;
#endif
	}

    private void LockGyroYaw()
    {
        //almost worked. next time have a "last rot/this rot" comparison and add the yaw component.
        startRotation *= Quaternion.AngleAxis( -Input.gyro.rotationRate.y , Vector3.forward);
        _webCamRenderer.enabled = true;
    }
	
	// Update is called once per frame
	void Update ()
	{
        _webCamRenderer.enabled = false;
        if(Input.GetMouseButton(0))
        {
            
            LockGyroYaw();
        }


        //almost there. just a bit of a mind fuck i guess
        Quaternion rot = ConvertRotation(Input.gyro.attitude);

       // rot = Quaternion.Euler(rot.eulerAngles.x, -rot.eulerAngles.y, rot.eulerAngles.z);

    //    transform.position = startRotation * startGyro * rot * Vector3.forward * -transform.position.magnitude;
        //transform.rotation = Quaternion.LookRotation(-transform.position.normalized, startRotation*rot * Vector3.up);
        rot = ConvertRotation(rot);
	    rot = Quaternion.Euler(-rot.eulerAngles.x, -rot.eulerAngles.y, rot.eulerAngles.z);

        rot = startRotation * rot;

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
        if (Input.GetMouseButton(0))
        {
            GUI.color = Color.red;
            GUILayout.Label("Gyro: " + Input.gyro.attitude.ToString());
            GUILayout.Label("Gyro Euler: " + Input.gyro.attitude.eulerAngles.ToString());
            GUI.color = Color.white;
        }
    }
}
