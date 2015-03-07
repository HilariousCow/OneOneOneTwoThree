using UnityEngine;
using System.Collections;

public class CameraAccelerometerControl : MonoBehaviour {

	// Use this for initialization
    public float SmoothAmount = 300f;
    public Renderer _webCamRenderer;

    private Quaternion keyboardRotation = Quaternion.identity;
    private Quaternion startRotation;
    private WebCamTexture webCamTexture;
	IEnumerator Start ()
	{
	    Input.gyro.enabled = true;
	    LockGyroYaw();
	    startRotation = Quaternion.AngleAxis(90f, Vector3.right);
        WebCamDevice[] devices = WebCamTexture.devices;

	    if (devices.Length > 0) {
		    webCamTexture =new WebCamTexture(320, 240, 12);
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
        
    }
	
	// Update is called once per frame
	void Update ()
	{
        
        if(Input.GetMouseButtonDown(0))
        {
            

            if (webCamTexture != null)
            {
            //    _webCamRenderer.enabled = true;
                //webCamTexture.Play();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {


            if (webCamTexture != null)
            {
                _webCamRenderer.enabled = false;
             //   webCamTexture.Stop();
            }
        }
        if(Input.GetMouseButton(0))
        {
            
            LockGyroYaw();
        }
	    keyboardRotation *= Quaternion.AngleAxis(Input.GetAxis("Horizontal") * Time.deltaTime*90f, Vector3.forward)*
                            Quaternion.AngleAxis(Input.GetAxis("Vertical") * Time.deltaTime * 90f, Vector3.right);

        //almost there. just a bit of a mind fuck i guess
        Quaternion rot = ConvertRotation(Input.gyro.attitude);

   
         rot = ConvertRotation(rot);
	    rot = Quaternion.Euler(-rot.eulerAngles.x, -rot.eulerAngles.y, rot.eulerAngles.z);

        rot = keyboardRotation*startRotation * rot;

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
