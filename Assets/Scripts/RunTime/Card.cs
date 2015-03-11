using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Stack StackPrefab;
    public Renderer RedrawCard;
    private CardSO _cardSoRef;
    private PlayerSO _playerSoRef;

    private Stack _previewStack;
    private Renderer _rend;
    private Collider _col;
    private Camera _cam;
    
    //drag behaviours
    private Transform _previousParentWhenDragging;
    private Vector3 _clickOriginOffset = Vector3.zero;
    private Vector3 _startDifferenceToCamera = Vector3.zero;
    private bool _playPreview = false;
    public PlayerSO PlayerSoRef
    {
        get { return _playerSoRef; }
    }

    public CardSO CardSoRef
    {
        get { return _cardSoRef; }
       
    }

    public void Init(CardSO cardSo, StackSO stackSo, PlayerSO playerSo)
	{
	    gameObject.name = "Card:"+cardSo.name;
        _cardSoRef = cardSo;
	    _playerSoRef = playerSo;
	    _rend = renderer;

	    RedrawCard.material = playerSo.CardMaterial;
        _rend.material = playerSo.CardMaterialRedraw;

	    _col = collider;

	    _cam = Camera.main;

        //this is to fake the void, but maybe void should just set its scale zero.
        
            _previewStack = transform.InstantiateChild(StackPrefab);
            _previewStack.Init(stackSo);

            _previewStack.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);
            Bounds stackBounds = _previewStack.transform.RenderBounds();

            _previewStack.transform.localPosition = Vector3.up*(stackBounds.size.y*0.5f + 0.125f);

            //_previewStack.PlayOperationAnimation();
        


	}



    void LateUpdate()
    {
        //hide from peekers

        float dot = Mathf.Clamp01(Vector3.Dot(Vector3.down, -transform.up)*2f);

       
        _previewStack.transform.localScale = Vector3.one*(1.0f-dot);

        if (Application.isEditor)
        {
            if ((!CardSoRef.FlipBottom && !CardSoRef.FlipTop && !CardSoRef.FlipStack) //if "nothing"
                || _playPreview)
            {
                _previewStack.PlayOperationAnimation(CardSoRef);
            }
        }
        else
        {
           //no hover over on mobile?
            _previewStack.PlayOperationAnimation(CardSoRef);
            
        }







    }

    #region IBeginDragHandler Members

    public void OnBeginDrag(PointerEventData eventData)
    {
       // Debug.Log("Beginning drag on" + gameObject.name);
        _col.enabled = false;
        _previousParentWhenDragging = transform.parent;
        transform.parent = null;
        _clickOriginOffset = transform.InverseTransformPoint(eventData.worldPosition);
        _startDifferenceToCamera = eventData.worldPosition - eventData.pressEventCamera.transform.position;
    }

    #endregion

    #region IDragHandler Members

    //really i need to do a different way as this only updates when the mouse moves or input changes, but
    //this doesn't take into account the camera moving.
    public void OnDrag(PointerEventData eventData)
    {
      //  

        Vector3 delta = transform.TransformPoint(_clickOriginOffset) - eventData.pressEventCamera.transform.position;

        Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);


        transform.position = eventData.pressEventCamera.transform.position + ray.direction * (_startDifferenceToCamera.magnitude );

        //todo: rule breaking
        transform.rotation = eventData.pressEventCamera.transform.rotation * Quaternion.AngleAxis(90f, Vector3.right);
    }

    #endregion

    #region IEndDragHandler Members

    //should really only reposition if you weren't reassigned by others.
    public void OnEndDrag(PointerEventData eventData)
    {
        _clickOriginOffset = Vector3.zero;
        _col.enabled = true;
        Debug.Log("Ended Drag" + gameObject.name);
        //ExecuteEvents.ExecuteHierarchy<IRefreshView>(_previousOwner.gameObject, null, (x, y) => x.RefreshView());//todo

        //need to snap back if we weren't claimed by any other thing.
        if (_previousParentWhenDragging.parent == transform.parent)//if our grandparent is the same, snap back? is that right? it might be a bit late in the evening for all this.
        {
            transform.SetParent(_previousParentWhenDragging);//redundant?
            transform.localPosition = Vector3.zero;


        }
        else if (transform.parent == null)//still no parent, meaning it didn't find a home, meaning it should snap back to where it came from
        {
            transform.parent = _previousParentWhenDragging;
            transform.ResetToParent();
        }

        _previousParentWhenDragging = null;
    }

    #endregion

    internal CardSlot GetSlotOwner()
    {
        if(_previousParentWhenDragging!=null)
        {
            return _previousParentWhenDragging.GetComponent<CardSlot>();
        }
        else
        {
            return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _playPreview = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _playPreview = false;
    }
}
