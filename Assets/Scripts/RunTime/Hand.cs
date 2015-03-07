using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//this is like an inventory.
public class Hand : MonoBehaviour
{
    public CardSlot CardSlotPrefab;

    private List<CardSlot> _slots;
    private Camera _cam;

    // Use this for initialization
	void Start ()
	{
	    _cam = Camera.main;
	    

	}
    public void Init(PlayerSO player, MatchSettingsSO matchSettings)
    {
        gameObject.name = player.name;
        //create slots based on match settings' card values
        _slots = new List<CardSlot>();
        foreach (CardSO cardSo in matchSettings.CardsPerHand)
        {
            CardSlot slot = transform.InstantiateChild(CardSlotPrefab);
            _slots.Add(slot);
        }

        //do a reorganize here.
        //added a renderer just in case. hopefully it being unenabled doesn't mean it doesn't have render bounds
    }


    internal void AddCardToHand(Card card)
    {
        List<CardSlot> closest = new List<CardSlot>(from CardSlot slot in _slots
                                                    orderby (slot.transform.position - card.transform.position).sqrMagnitude ascending
                                                    select slot);

        //find closest slot to incoming card

        foreach (CardSlot cardSlot in closest)
        {
            if (cardSlot.IsEmpty)
            {
                cardSlot.AddCardToSlot(card);
                break;
            }
            else
            {
                Card swapOut = cardSlot.RemoveCardFromSlot();
                cardSlot.AddCardToSlot(card);
                card = swapOut;
            }

        
        }
    }

	// Update is called once per frame
	void Update () {


	    foreach (var cardSlot in _slots)
	    {
            Quaternion faceDownRotation = Quaternion.identity;
	        Quaternion targetRotation = Quaternion.AngleAxis(-90f, Vector3.right);// *;
	        targetRotation = Quaternion.Inverse(transform.rotation)*
                              Quaternion.LookRotation(_cam.transform.position, Vector3.down) * targetRotation;
	        float dot = Vector3.Dot(-transform.position.normalized, _cam.transform.forward);
	        dot = Mathf.Pow(Mathf.Clamp01(dot), 4f);
	        float slerp = Mathf.SmoothStep(0f, 1f, dot);
	        cardSlot.transform.localRotation = Quaternion.Slerp(faceDownRotation, targetRotation, slerp);


	    }
        
	}

    


}
