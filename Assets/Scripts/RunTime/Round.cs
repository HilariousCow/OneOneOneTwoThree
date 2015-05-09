using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Round : MonoBehaviour {

    public CardSlot CardSlotScoreKeepingPrefab;

    
    private List<Hand> _hands;
    private Dictionary<Hand, CardSlot> _handToResultCardSlot;
    private Dictionary<Hand, int> _scoresForHands;
    private int _roundValue;

    public TextMesh RoundScoreAwarded;
    

    public int RoundValue
    {
        get { return _roundValue; }
    }

    public void Init(List<Hand> hands, int roundNumber, MatchSettingsSO gameSettings)
    {
        RoundScoreAwarded.text = "";
	    _hands = hands;
	    _roundValue = gameSettings.RoundScores[roundNumber];
        _handToResultCardSlot = new Dictionary<Hand, CardSlot>();
        _scoresForHands = new Dictionary<Hand, int>();
        foreach (Hand hand in _hands)
	    {
            CardSlot slot = transform.InstantiateChild<CardSlot>(CardSlotScoreKeepingPrefab);
            slot.transform.rotation = hand.transform.rotation;


            slot.gameObject.name = hand.gameObject.name + "_Round_" + (roundNumber + 1).ToString();
            slot.IsInteractive = false;//never interactive
            slot.HighlightIfEmpty(false);

            _handToResultCardSlot.Add(hand, slot);

            _scoresForHands.Add(hand, 0);
	    }
	}

  

    internal int GetScoreForHand(Hand hand)
    {
        return _scoresForHands[hand];


    }

    internal void ResolveRound(Card winningCard, Card losingCard)
    {
        Hand winningHand = _hands.Find(x => x.PlayerSoRef == winningCard.PlayerSoRef);
        Hand losingHand = _hands.Find(x => x.PlayerSoRef == losingCard.PlayerSoRef);

        CardSlot slotOfWinner = _handToResultCardSlot[winningHand];
        CardSlot slotOfLoser = _handToResultCardSlot[losingHand];

        Bounds boundsOfSlot = slotOfWinner.transform.RenderBounds();
        slotOfWinner.transform.position -= winningHand.transform.forward * (boundsOfSlot.size.z * .35f);

        slotOfLoser.transform.position = slotOfWinner.transform.position;
        slotOfWinner.transform.position += transform.up*0.125f;
        

        

        RoundScoreAwarded.text = _roundValue.ToString();
        List<Card> cards = new List<Card>(new Card[]{winningCard, losingCard});
        foreach (Card card in cards)
        {
            Hand handForCard = _hands.Find(x => x.PlayerSoRef == card.PlayerSoRef);
            CardSlot slot = _handToResultCardSlot[handForCard];
            slot.AddCardToSlot(card);
        }



        _scoresForHands[winningHand] += _roundValue;

    }
}
