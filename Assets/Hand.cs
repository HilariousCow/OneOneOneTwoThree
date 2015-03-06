using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour
{
    private CardSlot[] _slots;
    private Camera _cam;
	// Use this for initialization
	void Start ()
	{
	    _cam = Camera.main;
	    _slots = GetComponentsInChildren<CardSlot>();

	}
	
	// Update is called once per frame
	void Update () {


	    foreach (var cardSlot in _slots)
	    {
            Quaternion faceDownRotation = Quaternion.identity;
	        Quaternion targetRotation = Quaternion.AngleAxis(-90f, Vector3.right);// *;
	        targetRotation = Quaternion.Inverse(transform.rotation)*
                              Quaternion.LookRotation(_cam.transform.position, Vector3.down) * targetRotation;
	        float dot = Vector3.Dot(-transform.position.normalized, _cam.transform.forward);
	        dot = Mathf.Pow(Mathf.Clamp01(dot), 4f);
	        float slerp = Mathf.SmoothStep(0f, 1f, dot);
	        cardSlot.transform.localRotation = Quaternion.Slerp(faceDownRotation, targetRotation, slerp);


	    }
        
	}
}
