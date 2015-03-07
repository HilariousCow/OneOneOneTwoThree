using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//need to look at inventory code examples.
public class CardSlot : MonoBehaviour
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
	}

    public Card RemoveCardFromSlot()
    {
        Card removeMe = _card;
        removeMe.transform.parent = null;
        _card = null;
        return removeMe;
    }
}
