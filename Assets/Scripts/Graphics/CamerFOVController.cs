﻿using UnityEngine;
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
	// Use this for initialization
	void Start ()
	{
	    _cams = GetComponentsInChildren<Camera>();
	    _game = FindObjectOfType<MainGame>();

	    Bounds everything = _game.transform.RenderBounds();
	    MajorDistance = everything.size.magnitude*0.5f;
	    MinorDistance = everything.size.MinAxis();
	}
	
	// Update is called once per frame
	void Update () {

        float dot = Vector3.Dot(Vector3.down, transform.forward);

	    foreach (Camera cam in _cams)
	    {
            cam.fieldOfView = RelationshipBetweenSideAndTopFOV.Evaluate(dot);    
	    }
        transform.localPosition = Vector3.back * (Mathf.Lerp(MinorDistance, MajorDistance, 1f - dot) + AdditionalMindistance);

        float dotLR = Vector3.Dot(MainGameInstance.transform.forward, transform.forward);
	    dotLR = (dotLR + 1.0f)/2f;
        RenderSettings.fogColor = Color.Lerp(BlackSideFog, WhiteSideFog, dotLR);

	    
	}
}
