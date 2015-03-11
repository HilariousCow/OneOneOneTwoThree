﻿using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//this is like an inventory.
public class Hand : MonoBehaviour, IDropCardOnCardSlot
{
    public CardSlot CardSlotPrefab;
    public float Gap = 0.25f;
    private List<CardSlot> _slots;
    private Camera _cam;
    private PlayerSO _playerSoRef;

    public PlayerSO PlayerSoRef
    {
        get { return _playerSoRef; }
    }

    // Use this for initialization
	void Start ()
	{
	    _cam = Camera.main;
	    

	}
    public void Init(PlayerSO player, MatchSettingsSO matchSettings)
    {
        _playerSoRef = player;
        gameObject.name = PlayerSoRef.name;
        //create slots based on match settings' card values
        _slots = new List<CardSlot>();
        foreach (CardSO cardSo in matchSettings.CardsPerHand)
        {
            CardSlot slot = transform.InstantiateChild(CardSlotPrefab);
            slot.name = gameObject.name + "_Hand";
            _slots.Add(slot);
        }

        //do a reorganize here.
        _slots.PositionAlongLineCentered(Vector3.right, Gap, Vector3.zero);

        //added a renderer just in case. hopefully it being unenabled doesn't mean it doesn't have render bounds
    }


    internal void AddCardToHand(Card card)
    {

        CardSlot oldItemSlot = _slots.FirstOrDefault(x => x.Card == card);

        if (oldItemSlot != null)
        { //was already in a slot
            oldItemSlot.RemoveCardFromSlot();//free it up
        }
        List<CardSlot> closest = new List<CardSlot>(from CardSlot slot in _slots
                                                    orderby (slot.transform.position - card.transform.position).sqrMagnitude ascending
                                                    select slot);

        //find closest slot to incoming card

        foreach (CardSlot cardSlot in closest)
        {
            //repeat until no cards left to place
            if (cardSlot.IsEmpty)
            {
                cardSlot.AddCardToSlot(card);
                break;
            }
            else
            {
                Card swapOut = cardSlot.RemoveCardFromSlot();
                cardSlot.AddCardToSlot(card);
                Debug.Log("Swapping out" + swapOut.gameObject.name + " for " + card.gameObject.name);
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


    

    internal void RemoveCardFromHand(Card displacingCard)
    {
        CardSlot oldItemSlot = _slots.FirstOrDefault(x => x.Card == displacingCard);

        if(oldItemSlot!=null)
        {
            oldItemSlot.RemoveCardFromSlot();
        }
    }

    public void DroppedCardOnCardSlot(Card displacingCard, CardSlot targetSlot, CardSlot previousSlot)
    {
        if(!_slots.Contains(previousSlot))
        {
            previousSlot.RemoveCardFromSlot();
        }

        AddCardToHand(displacingCard);//this will deal with its own internal card re-moving but not for other people's
        
    }
}
namespace UnityEngine.EventSystems
{
    //Displaced item is the leftover item.
    public interface IDropCardOnCardSlot : IEventSystemHandler
    {
        void DroppedCardOnCardSlot(Card displacingCard, CardSlot targetSlot, CardSlot previousSlot);
    }

}