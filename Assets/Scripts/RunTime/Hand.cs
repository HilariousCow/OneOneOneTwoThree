using System.Linq;
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
    private Vector3 startingLocalPosition;
    public PlayerSO PlayerSoRef
    {
        get { return _playerSoRef; }
    }

    public List<CardSlot> Slots
    {
        get { return _slots; }
    }

    // Use this for initialization
	void Start ()
	{
	    _cam = Camera.main;

	    startingLocalPosition = transform.localPosition;
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
            Slots.Add(slot);
        }

        //do a reorganize here.
        Slots.PositionAlongLineCentered(transform.right, Gap, Vector3.zero);

        //added a renderer just in case. hopefully it being unenabled doesn't mean it doesn't have render bounds
    }


    internal void AddCardToHand(Card card)
    {

        CardSlot oldItemSlot = Slots.FirstOrDefault(x => x.CardInSlot == card);

        if (oldItemSlot != null)
        { //was already in a slot
            oldItemSlot.RemoveCardFromSlot();//free it up
        }
        List<CardSlot> closest = new List<CardSlot>(from CardSlot slot in Slots
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
          //      Debug.Log("Swapping out" + swapOut.gameObject.name + " for " + card.gameObject.name);
                card = swapOut;
                
            }

        
        }
    }

    public float handdot;
	// Update is called once per frame
	void Update () {

        //todo: move the entire anchor a bit toward the camera. need to stare base

	    Vector3 startingWorldPosition = transform.parent.TransformPoint(startingLocalPosition);

         handdot = Vector3.Dot(-startingWorldPosition.normalized, _cam.transform.forward);

        Vector3 targetPosition = _cam.transform.position.FlatY().normalized * startingLocalPosition.magnitude;
	    Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(targetPosition);
	    transform.localPosition
            = Vector3.Lerp(startingLocalPosition, targetLocalPosition, Mathf.Pow(Mathf.Clamp01(handdot), 2.0f));

        transform.LookAt(transform.position.y * Vector3.up, Vector3.up);
	    foreach (var cardSlot in Slots)
	    {
            Quaternion faceDownRotation = Quaternion.identity;
	        Quaternion targetRotation = Quaternion.AngleAxis(-90f, Vector3.right);
	        targetRotation = Quaternion.Inverse(transform.rotation)*
                              Quaternion.LookRotation(_cam.transform.position, cardSlot.transform.localPosition + transform.up * 60f) 
                              * targetRotation;
            /*targetRotation = Quaternion.Inverse(transform.rotation)*
                              Quaternion.LookRotation(_cam.transform.position, -_cam.transform.up) * targetRotation;*/
	        
            
            float dot = Vector3.Dot(-transform.position.normalized, _cam.transform.forward);
	        dot = Mathf.Pow(Mathf.Clamp01(dot), 0.8f);
	        float slerp = Mathf.SmoothStep(0f, 1f, dot);
	        cardSlot.transform.localRotation = Quaternion.Slerp(faceDownRotation, targetRotation, slerp);


	    }
        
	}


    

    internal void RemoveCardFromHand(Card displacingCard)
    {
        CardSlot oldItemSlot = Slots.FirstOrDefault(x => x.CardInSlot == displacingCard);

        if(oldItemSlot!=null)
        {
            oldItemSlot.RemoveCardFromSlot();
        }
    }

    public void DroppedCardOnCardSlot(Card displacingCard, CardSlot targetSlot, CardSlot previousSlot)
    {
        if(displacingCard.PlayerSoRef != PlayerSoRef)
        {
            return;
        }

        if(!Slots.Contains(previousSlot))
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

    public interface IPointerClickOnCard : IEventSystemHandler
    {
        void ClickedOnCard(Card card);
    }

}