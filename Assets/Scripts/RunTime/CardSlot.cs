using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//need to look at inventory code examples.
public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SlotHighlightEffect _highlightEffect;
    private bool _isInteractive = true;
    private Renderer _rend;
    private Card _cardInSlot;
    private bool _highlightIfEmpty = false;

    void Awake()
    {
        _highlightEffect = GetComponentInChildren<SlotHighlightEffect>();
    }

    void Start()
    {
        StopEffect();
    }

    public void HighlightIfEmpty(bool highlight)
    {
        _highlightIfEmpty = highlight;

        if (IsEmpty && _highlightIfEmpty)
        {
            StartEffect();
        }
        else
        {
            StopEffect();
        }
    }

    private void StartEffect()
    {
        _highlightEffect.StartEffect();
    }
    private void StopEffect()
    {
        _highlightEffect.StopEffect();
    }

    public bool IsEmpty
    {
        get { return CardInSlot == null; }
    }

    public Card CardInSlot
    {
        get { return _cardInSlot; }
    }

    public bool IsInteractive
    {
        get { return _isInteractive; }
        set
        {
            _isInteractive = value;
            if (CardInSlot != null)
            {
                CardInSlot.collider.enabled = value;
            }
        }
    }


    public void AddCardToSlot(Card card)
	{
	    _cardInSlot = card;
	    CardInSlot.transform.parent = transform;
        StopEffect();
	}

    public Card RemoveCardFromSlot()
    {
        Card removeMe = CardInSlot;
        removeMe.transform.parent = null;
        _cardInSlot = null;

        if(_highlightIfEmpty)
        {
            StartEffect();
        }
        return removeMe;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
         //   Debug.Log(eventData.pointerDrag.name + " Dropped On Reciever: " + gameObject.name);
            Card card = eventData.pointerDrag.GetComponent<Card>();
            //card.transform.position = eventData.worldPosition;
            if(card != null)
            {
                ExecuteEvents.ExecuteHierarchy<IDropCardOnCardSlot>(
                    transform.parent.gameObject,
                    null,
                    (x, y) => x.DroppedCardOnCardSlot(card, this, card.GetSlotOwner())
                    );

                if (!IsInteractive) card.CachedCollider.enabled = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.localScale = Vector3.one*1.2f;// Mathf.Lerp(1f, 1.1f, (1.0f + Mathf.Sin(Time.time * Mathf.PI * 2.0f)) * 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //transform.localScale = Vector3.one;
    }



    #region IPointerClickHandler Members

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractive)
        {
            //fail beep
            return;
        }
        if (CardInSlot != null)
        {
            ExecuteEvents.ExecuteHierarchy<IPointerClickOnCard>(
                transform.parent.gameObject,
                null,
                (x, y) => x.ClickedOnCard(CardInSlot)
                );
        }
    }

    #endregion

    internal void ShowSlot(bool p)
    {
        renderer.enabled = p;
    }
}

