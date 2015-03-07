using UnityEngine;
using System.Collections;

public class CamerFOVController : MonoBehaviour {

    public AnimationCurve RelationshipBetweenSideAndTopFOV;

    private Camera _cam;
	// Use this for initialization
	void Start () {
        _cam = camera;
	}
	
	// Update is called once per frame
	void Update () {

        float dot = Vector3.Dot(Vector3.down, transform.forward);

        _cam.fieldOfView = RelationshipBetweenSideAndTopFOV.Evaluate(dot);

	    float dotLR = Vector3.Dot(Vector3.right, transform.forward);
	    dotLR = (dotLR*0.5f + 1.0f)/2f;
	    RenderSettings.fogColor = Color.Lerp(Color.black, Color.white, dotLR);
	}
}
