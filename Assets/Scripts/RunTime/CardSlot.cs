using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//need to look at inventory code examples.
public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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
            card.transform.position = eventData.worldPosition;
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
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}

