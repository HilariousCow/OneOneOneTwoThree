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
    private Camera _cam;
    
    //drag behaviours
    private Transform _previousParentWhenDragging;
    private Vector3 _clickOriginOffset = Vector3.zero;
    private Vector3 _startDifferenceToCamera = Vector3.zero;

    public PlayerSO PlayerSoRef
    {
        get { return _playerSoRef; }
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


        if (_cardSoRef.FlipBottom || _cardSoRef.FlipTop || _cardSoRef.FlipStack)
        {
            _previewStack = transform.InstantiateChild(StackPrefab);
            _previewStack.Init(stackSo);

            _previewStack.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);
            Bounds stackBounds = _previewStack.transform.RenderBounds();

            _previewStack.transform.localPosition = Vector3.up*(stackBounds.size.y*0.5f + 0.125f);

            _previewStack.PlayOperationAnimation();
        }


	}



    void LateUpdate()
    {
        //hide from peekers

        float dot = Mathf.Clamp01(Vector3.Dot(Vector3.down, -transform.up)*2f);
        if(_previewStack != null)
        {
            _previewStack.transform.localScale = Vector3.one*(1.0f-dot);
        }


        //todo: this should be done in the stack. get IT to trigger animations.
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
       // Debug.Log("Beginning drag on" + gameObject.name);
        _col.enabled = true;
        _previousParentWhenDragging = transform.parent;
        transform.parent = null;
        _clickOriginOffset = transform.InverseTransformPoint(eventData.worldPosition);
        _startDifferenceToCamera = eventData.worldPosition - eventData.pressEventCamera.transform.position;
    }

    #endregion

    #region IDragHandler Members

    public void OnDrag(PointerEventData eventData)
    {
      //  

        Vector3 delta = transform.TransformPoint(_clickOriginOffset) - eventData.pressEventCamera.transform.position;

        Ray ray = eventData.pressEventCamera.ScreenPointToRay(eventData.position);


        transform.position = eventData.pressEventCamera.transform.position + ray.direction * (_startDifferenceToCamera.magnitude + 10f);

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
