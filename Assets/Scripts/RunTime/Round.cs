using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Round : MonoBehaviour {

    public CardSlot CardSlotScoreKeepingPrefab;

    
    private List<Hand> _hands;
    private Dictionary<Hand, CardSlot> _handToResultCardSlot;
    private Dictionary<Hand, int> _scoresForHands;
    private int _roundValue;
    private int _numberOfRounds;

    public TextMesh RoundScoreAwarded;

    private Bounds _cardBoundsAtStart;

    public int RoundValue
    {
        get { return _roundValue; }
    }

    public void Init(List<Hand> hands, int roundNumber, MatchSettingsSO gameSettings)
    {
        RoundScoreAwarded.text = "";
	    _hands = hands;
	    _roundValue = gameSettings.RoundScores[roundNumber];
        _numberOfRounds = gameSettings.NumberOfRounds;
        _handToResultCardSlot = new Dictionary<Hand, CardSlot>();
        _scoresForHands = new Dictionary<Hand, int>();

        foreach (Hand hand in _hands)
	    {
            CardSlot slot = transform.InstantiateChild<CardSlot>(CardSlotScoreKeepingPrefab);
            _cardBoundsAtStart = slot.transform.RenderBounds();
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

    internal IEnumerator ResolveRound(Card winningCard, Card losingCard)
    {
        Hand winningHand = _hands.Find(x => x.PlayerSoRef == winningCard.PlayerSoRef);
        Hand losingHand = _hands.Find(x => x.PlayerSoRef == losingCard.PlayerSoRef);

        CardSlot slotOfWinner = _handToResultCardSlot[winningHand];
        CardSlot slotOfLoser = _handToResultCardSlot[losingHand];


        slotOfWinner.transform.localPosition -= transform.parent.InverseTransformDirection(winningHand.transform.forward) * (_cardBoundsAtStart.size.z * 1f);

        slotOfLoser.transform.localPosition = slotOfWinner.transform.localPosition;

        slotOfWinner.transform.localPosition += Vector3.up * 0.25f;
        //offset to reveal a point.

        float baseGap = _cardBoundsAtStart.size.z / 7f;//blank space either side of the dots
        float scoreGap = (_cardBoundsAtStart.size.z - (baseGap * 2f)) 
            * ((float)_roundValue / (float)_numberOfRounds);
        slotOfWinner.transform.localPosition -= transform.parent.InverseTransformDirection( winningHand.transform.forward) * (baseGap + scoreGap);

        

        RoundScoreAwarded.text = _roundValue.ToString();
        RoundScoreAwarded.transform.localPosition = Vector3.up*2f;
        RoundScoreAwarded.transform.localRotation =
            Quaternion.LookRotation(transform.parent.InverseTransformDirection(winningHand.transform.forward)) * Quaternion.AngleAxis(90f, Vector3.right);

     //   RoundScoreAwarded.transform.localPosition = slotOfWinner.transform.localPosition + transform.parent.InverseTransformDirection(-winningHand.transform.forward * _cardBoundsAtStart.size.z * 1f);
        /*RoundScoreAwarded.transform.localPosition = Vector3.Reflect(RoundScoreAwarded.transform.localPosition,
                                                                    Vector3.forward);*/


      /*  List<Card> cards = new List<Card>(new Card[]{winningCard, losingCard});
        foreach (Card card in cards)
        {
            Hand handForCard = _hands.Find(x => x.PlayerSoRef == card.PlayerSoRef);
            CardSlot slot = _handToResultCardSlot[handForCard];
            slot.AddCardToSlot(card);
            yield return new WaitForSeconds(0.5f);
        }*/

        slotOfLoser.AddCardToSlot(losingCard);
        yield return new WaitForSeconds(0.5f);
        slotOfWinner.AddCardToSlot(winningCard);


        losingCard.PreviewStack.gameObject.SetActive(false);
        winningCard.PreviewStack.gameObject.SetActive(false);
        _scoresForHands[winningHand] += _roundValue;

    }
}
