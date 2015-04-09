using UnityEngine;
using System.Collections;

public class CamerFOVController : MonoBehaviour
{

    public MainGame MainGameInstance;
    public AnimationCurve RelationshipBetweenSideAndTopFOV;
    public float AdditionalMindistance = 10;
    public Color BlackSideFog;
    public Color WhiteSideFog;
    private Camera[] _cams;
    private MainGame _game;

    private float MajorDistance;
    private float MinorDistance;
    private Bounds _everything;
    // Use this for initialization
	void Start ()
	{
	    _cams = GetComponentsInChildren<Camera>();
	    _game = FindObjectOfType<MainGame>();

	    _everything = _game.transform.RenderBounds();
	    MajorDistance = _everything.size.magnitude*2f;
	    MinorDistance = _everything.size.MinAxis();
	}
	
	// Update is called once per frame
	void Update () {

        float dot = Vector3.Dot(Vector3.down, transform.forward);
        float sideDot = Mathf.Abs( Vector3.Dot(Vector3.down, transform.forward) );

        float funDot = Mathf.Pow(Mathf.Clamp01(dot ), 0.6f);
        transform.localPosition = Vector3.back * (Mathf.Lerp(MinorDistance, MajorDistance, 1f - funDot) + AdditionalMindistance);

        float fovForEverything =  _everything.size.magnitude * Camera.main.aspect;

        fovForEverything = 1.45f*Mathf.Rad2Deg * Mathf.Atan2(fovForEverything, transform.position.magnitude);
        foreach (Camera cam in _cams)
        {
            cam.fieldOfView = fovForEverything;// RelationshipBetweenSideAndTopFOV.Evaluate(dot);
            cam.nearClipPlane = Mathf.Clamp(transform.localPosition.magnitude-100f, 0.1f, 10000f);
            cam.farClipPlane = Mathf.Clamp(transform.localPosition.magnitude + 100f, 0.1f, 10000f);
        }

        float dotLR = Vector3.Dot(MainGameInstance.transform.forward, transform.forward);
	    dotLR = (dotLR + 1.0f)/2f;
        RenderSettings.fogColor = Color.Lerp(BlackSideFog, WhiteSideFog, dotLR);
        RenderSettings.fogMode = FogMode.Linear;
	    RenderSettings.fogStartDistance = Camera.main.nearClipPlane;
        RenderSettings.fogEndDistance = Camera.main.farClipPlane;


	}
}
