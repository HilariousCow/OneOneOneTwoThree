using UnityEngine;
using System.Collections;

public class CamerFOVController : MonoBehaviour
{

    public AnimationCurve RelationshipBetweenSideAndTopFOV;
    public float AdditionalMindistance = 10;
    public Color BlackSideFog;
    public Color WhiteSideFog;
    public Camera FirstCamera;
    private Camera[] _cams;
    private MainGame _game;

    private float MajorDistance;
    private float MinorDistance;
    private Bounds _everything;

    
    // Use this for initialization
	void Awake ()
	{
	    _cams = GetComponentsInChildren<Camera>();
	    _game = FindObjectOfType<MainGame>();

        _everything = new Bounds(Vector3.zero, Vector3.one * 100f);
	}

    public void SetMainGame(MainGame mainGame)
    {
        _game = mainGame;
        UpdateDistances();
    }
    private void UpdateDistances()
    {

        _everything = _game.transform.RenderBounds();
	    MajorDistance = _everything.size.magnitude*2f;
	    MinorDistance = _everything.size.MinAxis();
	}


    private float forwardTraceDist;

    private float posVerticalDist;
    private float posHorizontalDist;
    private float negVerticalDist;
    private float negHorizontalDist;

    private Vector3 forwardHitPos = Vector3.zero;

    private Vector3 posVerticalHitPos = Vector3.zero;
    private Vector3 posHorizontalHitPos = Vector3.zero;

    private Vector3 negVerticalHitPos = Vector3.zero;
    private Vector3 negHorizontalHitPos = Vector3.zero;



	// Update is called once per frame
	void Update ()
	{

     

        float dot = Vector3.Dot(Vector3.down, transform.forward);

        float funDot = Mathf.Pow(Mathf.Clamp01(dot ), 0.6f);
        transform.localPosition = Vector3.back * (Mathf.Lerp(MinorDistance, MajorDistance, 1f - funDot) + AdditionalMindistance);

        float fovForEverything =  _everything.size.magnitude * Camera.main.aspect;



	    float rollDot = Mathf.Abs(Vector3.Dot(transform.right, Vector3.down));
	    float fovRoll = Mathf.Lerp(1.32f, 0.78f, rollDot);
	    float radiusOfEverything = _everything.size.magnitude ;
        fovForEverything = Mathf.Rad2Deg * Mathf.Atan2(fovForEverything, transform.position.magnitude) * fovRoll;

	   // fovForEverything = 90f;///TEEEEEMPPP!!
        //tween to target fov
	    float fovDelta = Mathf.Abs(_cams[0].fieldOfView - fovForEverything);
	    fovForEverything = Mathf.MoveTowards(_cams[0].fieldOfView, fovForEverything, fovDelta*Time.deltaTime*5.0f);
        foreach (Camera cam in _cams)
        {
            cam.fieldOfView = fovForEverything;// RelationshipBetweenSideAndTopFOV.Evaluate(dot);
            cam.nearClipPlane = Mathf.Clamp(transform.localPosition.magnitude - radiusOfEverything, 0.1f, 10000f);
            cam.farClipPlane = Mathf.Clamp(transform.localPosition.magnitude + radiusOfEverything, 0.1f, 10000f);
        }

       
         
        RenderSettings.fogMode = FogMode.Linear;
	    RenderSettings.fogStartDistance = Mathf.Lerp( Camera.main.nearClipPlane, Camera.main.farClipPlane, 0.5f);
	    RenderSettings.fogEndDistance = Mathf.Lerp(Camera.main.nearClipPlane, Camera.main.farClipPlane, 0.7f);

        //this needs to be an event which the game triggers.
        if (_game != null)
        {
            Vector4 targetColor = _game.MainStack.GetTopTokenSide() == TokenSide.White
                                      ? WhiteSideFog
                                      : BlackSideFog;
            Vector4 color = FirstCamera.backgroundColor;
            color = Vector3.MoveTowards(color, targetColor, ((targetColor - color).magnitude + 0.1f)*Time.deltaTime*5f);
            FirstCamera.backgroundColor = color;

            Vector4 oppositeColor = Vector4.one - color;
            oppositeColor.w = 1f;
            Shader.SetGlobalColor("_OppositeOfBGColor", oppositeColor);
            Shader.SetGlobalColor("_BGColor", color);
            RenderSettings.fogColor = color;
        }
        else
        {
            Vector4 color = FirstCamera.backgroundColor;
            
            FirstCamera.backgroundColor = color;

            Vector4 oppositeColor = Vector4.one - color;
            oppositeColor.w = 1f;
            Shader.SetGlobalColor("_OppositeOfBGColor", oppositeColor);
            Shader.SetGlobalColor("_BGColor", color);
            RenderSettings.fogColor = color;
        }
	   // transform.rotation = Quaternion.LookRotation(_game.MainStack.transform.position - transform.position, transform.up);
	}

    void OnDrawGizmos()
    {

    
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_everything.center, _everything.size);
        
        Gizmos.color = Color.white;
    }
}
