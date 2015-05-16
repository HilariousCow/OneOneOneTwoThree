using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;



public class SliderBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public Transform Outside;

    internal AIPlayer SelectedObject;
    internal List<AIPlayer> AIs;


    private bool _isDragging = false;
    private int _dragPointerID = -1;
    private Vector3 _localStartDragPos;


    public void Init(List<AIPlayer> items)
    {


        AIs = new List<AIPlayer>(items);
        SelectedObject = GetCurrentlySelectedObject();
    }

    private AIPlayer GetCurrentlySelectedObject()
    {
        return (from AIPlayer n in AIs orderby Vector3.Dot(n.transform.up, transform.up) select n).FirstOrDefault();
    }

    private int GetSelectedIndex()
    {
        return AIs.IndexOf(GetCurrentlySelectedObject());
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	    if (_isDragging)
	    {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (_dragPointerID != -1)
            {
                Touch touch = Input.GetTouch(_dragPointerID);
                ray = Camera.main.ScreenPointToRay(touch.position);
            }
	    }
	    else
	    {
	        Quaternion targetDelta = Quaternion.Inverse(SelectedObject.transform.localRotation)*Quaternion.identity;
	        float angle = Quaternion.Angle(targetDelta, Quaternion.identity);
            if(angle > 0f)
            {
                Outside.transform.localRotation = Quaternion.Slerp(Outside.transform.localRotation, targetDelta,
                                                                   Time.deltaTime*angle*5f);
            }

	    }
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragPointerID = eventData.pointerId;

        _localStartDragPos =transform.InverseTransformPoint(  eventData.worldPosition );
    }
     
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 currentLocalPos = transform.InverseTransformPoint( eventData.worldPosition );
        Vector3 delta = currentLocalPos - _localStartDragPos;

        Outside.localRotation *= Quaternion.AngleAxis(delta.x * 360, Vector3.forward);


        _localStartDragPos = currentLocalPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _dragPointerID = -1;

        SelectedObject = GetCurrentlySelectedObject();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

   
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

  
    
}
