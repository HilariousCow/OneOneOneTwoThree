using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;



public class SliderBox<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler where T : Object
{
    //erk :(
    public SliderOption<T> AIVizPrefab;
    public Transform Outside;

    internal SliderOption<T> SelectedObject;
    internal List<SliderOption<T>> AIs;


    private bool _isDragging = false;
    private int _dragPointerID = -1;
    private Vector3 _localStartDragPos;


    public void Init(List<T> items)
    {
        AIs = new List<SliderOption<T>>();
        for (int index = 0; index < items.Count; index++)
        {
            float fraction = (float)index/items.Count - 1;
            fraction *= Mathf.PI*2f;
            Vector3 pos = new Vector3(Mathf.Sin(fraction), Mathf.Cos(fraction), 0.0f) * 2f;


            T aiPlayer = items[index];
            SliderOption<T> viz = Outside.transform.InstantiateChild(AIVizPrefab);
           
            viz.Init(aiPlayer);

            viz.transform.localPosition = pos;
            viz.transform.localRotation = Quaternion.LookRotation(Vector3.forward, pos);


            AIs.Add(viz);
        }

        SelectedObject = GetCurrentlySelectedObject();
    }

    public SliderOption<T> GetCurrentlySelectedObject()
    {
        return (from SliderOption<T> n in AIs orderby Vector3.Dot(n.transform.up, transform.up) ascending select n).LastOrDefault();
    }

  

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

/*	    if (_isDragging)
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

	    }*/
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

        Outside.localRotation *= Quaternion.AngleAxis(-delta.x * 30, Vector3.forward);


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
