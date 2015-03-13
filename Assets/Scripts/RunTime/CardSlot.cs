using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//need to look at inventory code examples.
public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Renderer _rend;
    private Card _card;
    public bool IsEmpty
    {
        get { return Card == null; }
    }

    public Card Card
    {
        get { return _card; }
    }

    public void AddCardToSlot(Card card)
	{
        

	    _card = card;
	    Card.transform.parent = transform;
        //Card.transform.ResetToParent();
	}

    public Card RemoveCardFromSlot()
    {
        Card removeMe = Card;
        removeMe.transform.parent = null;
        _card = null;
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
        if (Card != null)
        {
            ExecuteEvents.ExecuteHierarchy<IPointerClickOnCard>(
                transform.parent.gameObject,
                null,
                (x, y) => x.ClickedOnCard(Card)
                );
        }
    }

    #endregion
}

