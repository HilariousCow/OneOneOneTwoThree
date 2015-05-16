using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SpinTable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 _lastDragPos;
    
    private Camera _cam;
    private bool _allowDrag = false;

    void Start()
    {
        _cam = _cam ?? Camera.main;
    }

    #region IBeginDragHandler Members

    public void OnBeginDrag(PointerEventData eventData)
    {
        _allowDrag = Vector3.Dot(Vector3.down, _cam.transform.forward) > 0.707f;
        _lastDragPos = eventData.worldPosition;
    }

    #endregion

    public void OnDrag(PointerEventData eventData)
    {
      /*  if (!_allowDrag)
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
        _lastDragPos = eventData.worldPosition;*/
    }

    //todo: LateUpdate so it drags when you move it.

   
    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

   
    //always rotate table to be pointing to camera up
    public void LateUpdate()
    {
       float autoCalibrateAmount =  (Vector3.Dot(Vector3.down, _cam.transform.forward) - 0.707f) / ( 1f - 0.707f);
        autoCalibrateAmount = Mathf.Clamp01(autoCalibrateAmount);

        autoCalibrateAmount = Mathf.Pow(autoCalibrateAmount, 2f);

        Quaternion targetRotation = Quaternion.LookRotation(_cam.transform.up.FlatY().normalized, Vector3.up);


            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                                                            (angle + 1f) * Time.deltaTime * autoCalibrateAmount * 5f);
       
    }
}
