using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Stack StackPrefab;
    public Renderer RedrawCard;
    private CardSO _cardSoRef;
    private PlayerSO _playerSoRef;

    private Stack _previewStack;
    private Renderer _rend;
    private Collider _col;

    //drag behaviours
    private Transform _previousParentWhenDragging;
	public void Init(CardSO cardSo, StackSO stackSo, PlayerSO playerSo)
	{
	    gameObject.name = "Card:"+cardSo.name;
	    _cardSoRef = cardSo;
	    _playerSoRef = playerSo;
	    _rend = renderer;

	    RedrawCard.material = playerSo.CardMaterial;
        _rend.material = playerSo.CardMaterialRedraw;

	    _col = collider;




        if (_cardSoRef.FlipBottom || _cardSoRef.FlipTop || _cardSoRef.FlipStack)
        {
            _previewStack = transform.InstantiateChild(StackPrefab);
            _previewStack.Init(stackSo);

            _previewStack.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);
            Bounds stackBounds = _previewStack.transform.RenderBounds();

            _previewStack.transform.localPosition = Vector3.up*(stackBounds.size.y*0.5f + 0.125f);
        }


	}



    void LateUpdate()
    {
        if (_cardSoRef.FlipStack)
        {
            _previewStack.transform.localRotation = _previewStack.transform.localRotation*
                                                    Quaternion.AngleAxis(Time.deltaTime*360f, Vector3.forward);
        }
        if(_cardSoRef.FlipTop)
        {
            
        }
        if (_cardSoRef.FlipBottom)
        {

        }
    }

    #region IBeginDragHandler Members

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Beginning drag on" + gameObject.name);
        _col.enabled = true;
        _previousParentWhenDragging = transform.parent;
        transform.parent = null;
    }

    #endregion

    #region IDragHandler Members

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging on" + gameObject.name);

        Vector3 delta = transform.position - eventData.pressEventCamera.transform.position;

        Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);
        
        transform.position = ray.GetPoint(delta.magnitude);
    }

    #endregion

    #region IEndDragHandler Members

    public void OnEndDrag(PointerEventData eventData)
    {
        _col.enabled = true;

        //ExecuteEvents.ExecuteHierarchy<IRefreshView>(_previousOwner.gameObject, null, (x, y) => x.RefreshView());//todo

        //need to snap back if we weren't claimed by any other thing.
        if (_previousParentWhenDragging.parent == transform.parent)//if our grandparent is the same, snap back? is that right? it might be a bit late in the evening for all this.
        {
            transform.SetParent(_previousParentWhenDragging);//redundant?
            transform.localPosition = Vector3.zero;
            _previousParentWhenDragging = null;

        }
        else if (transform.parent == null)//still no parent, meaning it didn't find a home, meaning it should snap back to where it came from
        {
            transform.parent = _previousParentWhenDragging;
            transform.ResetToParent();
        }
    }

    #endregion
}
