using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//need to look at inventory code examples.
public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{

    private Card _card;
    public bool IsEmpty
    {
        get { return _card == null; }
    }
	public void AddCardToSlot(Card card)
	{
	    _card = card;
	    _card.transform.parent = transform;
        _card.transform.ResetToParent();
	}

    public Card RemoveCardFromSlot()
    {
        Card removeMe = _card;
        removeMe.transform.parent = null;
        _card = null;
        return removeMe;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Debug.Log("Dropped On Reciever: " + eventData.pointerDrag.name);
            Card item = eventData.pointerDrag.GetComponent<Card>();
            if (item != null)
            {
                AddCardToSlot(item);
            }
            //
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
