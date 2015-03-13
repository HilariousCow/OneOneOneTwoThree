using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SpinTable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 _lastDragPos;
    public GameObject[] SpinThing;
    private bool _allowDrag = false;
    #region IBeginDragHandler Members

    public void OnBeginDrag(PointerEventData eventData)
    {
        _allowDrag = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
        _lastDragPos = eventData.worldPosition;
    }

    #endregion

    public void OnDrag(PointerEventData eventData)
    {
        if (!_allowDrag)
        {
            _lastDragPos = eventData.worldPosition;
            return;
        }
        
        Vector3 pos = eventData.worldPosition;
        Vector3 delta = pos - _lastDragPos;
        if (delta.magnitude > 0f)
        {
             //that's 2d?
            Vector3 startPos = _lastDragPos;

            Quaternion startRot = Quaternion.LookRotation(startPos.FlatY(), Vector3.up);
            Quaternion endRot = Quaternion.LookRotation(pos.FlatY(), Vector3.up);

            Quaternion rot = Quaternion.Inverse(startRot)*endRot;

            foreach (var o in SpinThing)
            {
                o.transform.rotation *= rot;
            }

            
        }
        _lastDragPos = eventData.worldPosition;
    }

    //todo: LateUpdate so it drags when you move it.

    #region IEndDragHandler Members

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    #endregion

   
}
