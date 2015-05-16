using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;



public class SliderBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool _isDragging = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	    if (_isDragging)
	    {
	        
	    }
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }
     
    public void OnDrag(PointerEventData eventData)
    {
       
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
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
