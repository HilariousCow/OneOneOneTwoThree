﻿using UnityEngine;
using System.Collections;

public class CamerFOVController : MonoBehaviour
{

    public MainGame MainGameInstance;
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

        UpdateDistances();

        float dot = Vector3.Dot(Vector3.down, transform.forward);

        float funDot = Mathf.Pow(Mathf.Clamp01(dot ), 0.6f);
        transform.localPosition = Vector3.back * (Mathf.Lerp(MinorDistance, MajorDistance, 1f - funDot) + AdditionalMindistance);

      /*   posVerticalDist = Mathf.Abs( Vector3.Dot(Camera.main.transform.up, _everything.size));
         posHorizontalDist = Mathf.Abs( Vector3.Dot(Camera.main.transform.right, _everything.size) * Camera.main.aspect);
        */
        //need to go both sides


        //try closest point on bounds.

	    Vector3 closestPointOnBounds = Camera.main.transform.position;
        
	    closestPointOnBounds.x = Mathf.Clamp(closestPointOnBounds.x, _everything.min.x, _everything.max.x);
        closestPointOnBounds.y = Mathf.Clamp(closestPointOnBounds.y, _everything.min.y, _everything.max.y);
        closestPointOnBounds.z = Mathf.Clamp(closestPointOnBounds.z, _everything.min.z, _everything.max.z);

	    Vector3 delta = (closestPointOnBounds - Camera.main.transform.position);
        Vector3 reverseStartPos = (delta.normalized * delta.magnitude * 1.1f )+ Camera.main.transform.position;
        Ray forwarTrace = new Ray(Camera.main.transform.position , reverseStartPos - Camera.main.transform.position);
        if (_everything.IntersectRay(forwarTrace, out forwardTraceDist))
        {
            forwardHitPos = forwarTrace.GetPoint(forwardTraceDist);
            forwardHitPos = Vector3.Lerp(forwardHitPos, _everything.center, 0.1f);//aim just inward so that there's always a clean trace
        }
        else forwardHitPos = Vector3.zero;

       
        Ray positiveVerticalRay = new Ray((Camera.main.transform.up * 1000f) + forwardHitPos, forwardHitPos - (Camera.main.transform.up * 1000f) );
        Ray positiveHorizontalRay = new Ray((Camera.main.transform.right * 1000f )+ forwardHitPos, forwardHitPos - (Camera.main.transform.right * 1000f) );

        Ray negativeVerticalRay = new Ray((-Camera.main.transform.up * 1000f) + forwardHitPos, forwardHitPos - (-Camera.main.transform.up * 1000f) );
        Ray negativeHorizontalRay = new Ray((-Camera.main.transform.right * 1000f) + forwardHitPos, forwardHitPos - (-Camera.main.transform.right * 1000f) );

       
        if (_everything.IntersectRay(positiveVerticalRay, out posVerticalDist ))
        {
            posVerticalHitPos = positiveVerticalRay.GetPoint(posVerticalDist);
            posVerticalDist = posVerticalHitPos.magnitude;
        }

        if (_everything.IntersectRay(positiveHorizontalRay, out posHorizontalDist))
        {
            posHorizontalHitPos = positiveHorizontalRay.GetPoint(posHorizontalDist);
            posHorizontalDist = posHorizontalHitPos.magnitude;
        }

        //negs

        if (_everything.IntersectRay(negativeVerticalRay, out negVerticalDist))
        {
            negVerticalHitPos = negativeVerticalRay.GetPoint(negVerticalDist);
            negVerticalDist = negVerticalHitPos.magnitude;
        }

        if (_everything.IntersectRay(negativeHorizontalRay, out negHorizontalDist))
        {
            negHorizontalHitPos = negativeHorizontalRay.GetPoint(negHorizontalDist);
            negHorizontalDist = negHorizontalHitPos.magnitude;
        }

        float fovForEverything = Mathf.Max(Mathf.Max(posVerticalDist, negVerticalDist), 
            Mathf.Max(posHorizontalDist / Camera.main.aspect,
            negHorizontalDist / Camera.main.aspect));
        //float fovForEverything =  _everything.size.magnitude * Camera.main.aspect;




	    float radiusOfEverything = _everything.size.magnitude ;
        fovForEverything = Mathf.Rad2Deg * Mathf.Atan2(fovForEverything,  transform.position.magnitude) *2f;

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

        float dotLR = Vector3.Dot(MainGameInstance.transform.forward, transform.forward);
	    dotLR = (dotLR + 1.0f)/2f;
        
         
        RenderSettings.fogMode = FogMode.Linear;
	    RenderSettings.fogStartDistance = Mathf.Lerp( Camera.main.nearClipPlane, Camera.main.farClipPlane, 0.5f);
	    RenderSettings.fogEndDistance = Mathf.Lerp(Camera.main.nearClipPlane, Camera.main.farClipPlane, 0.7f);

        //this needs to be an event which the game triggers.
        if(MainGameInstance != null)
        {
            Vector4 targetColor = MainGameInstance.MainStack.GetTopTokenSide() == TokenSide.White
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
	   // transform.rotation = Quaternion.LookRotation(_game.MainStack.transform.position - transform.position, transform.up);
	}

    void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(forwardHitPos, Camera.main.transform.position);
        Gizmos.DrawCube(forwardHitPos, Vector3.one * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(forwardHitPos, posVerticalHitPos);
        Gizmos.DrawCube(posVerticalHitPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(forwardHitPos, negVerticalHitPos);
        Gizmos.DrawCube(negVerticalHitPos, Vector3.one * 0.5f);


        Gizmos.color = Color.red;
        Gizmos.DrawLine(forwardHitPos, posHorizontalHitPos);
        Gizmos.DrawCube(posHorizontalHitPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(forwardHitPos, negHorizontalHitPos);
        Gizmos.DrawCube(negHorizontalHitPos, Vector3.one * 0.5f);


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_everything.center, _everything.size);
        
        Gizmos.color = Color.white;
    }
}
