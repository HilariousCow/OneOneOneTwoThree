using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreHand : MonoBehaviour
{
    public CardSlot CardSlotScoreKeepingPrefab;


    public bool FinishedRound { get { return roundNumber == numberOfRounds; } }
    public bool GameIsATie { get
        {
            
            //reorder the scores from highest to lowest
            List<KeyValuePair<Hand, int>> sortedDict = (from entry in TotalledScores orderby entry.Value descending select entry).ToList();
            int startScore = sortedDict[0].Value;
            int numTiedForWin = 0;
            foreach (KeyValuePair<Hand, int> keyValuePair in sortedDict)
            {
                numTiedForWin++;
                if(keyValuePair.Value < startScore)//no longer equal
                {
                    break;
                }
            }

            return numTiedForWin > 1;
        } 
    }

    public Dictionary<Hand, int> TotalledScores
    {
        get
        {
            Dictionary<Hand, int> scores  = new Dictionary<Hand, int>();
            foreach (Hand hand in _scores.Keys)
            {
                List<int> score = _scores[hand];

                scores.Add(hand, score.Sum());
            }
            return scores;
        }
    }
    private Dictionary<Hand, List<CardSlot>> _roundScoresPerHand;
    private Dictionary<Hand, List<int>> _scores;

    private List<int> _roundScores;
    private int roundNumber = 0;
    private int numberOfRounds = 5;

    
    internal void Init(List<Hand> _hands, MatchSettingsSO _matchSettings)
    {
        roundNumber = 0;
        numberOfRounds = _matchSettings.NumberOfRounds;

        List<CardSO> _setOfCardsForMatch = new List<CardSO>(_matchSettings.CardsPerHand);
        _roundScoresPerHand = new Dictionary<Hand, List<CardSlot>>();
        _scores = new Dictionary<Hand, List<int>>();
        _roundScores = new List<int>(_matchSettings.RoundScores);
        foreach (Hand hand in _hands)
        {
            List<CardSlot> trackers = new List<CardSlot>();
            List<int> scores = new List<int>();
            for (int index = 0; index < numberOfRounds; index++)
            {
                CardSO cardSo = _setOfCardsForMatch[index];
                CardSlot slot = transform.InstantiateChild<CardSlot>(CardSlotScoreKeepingPrefab);
                slot.transform.rotation = hand.transform.rotation;

                
                slot.gameObject.name = hand.gameObject.name + "_Round_" + (index + 1).ToString();
                trackers.Add(slot);

                scores.Add(0);
            }
            _scores.Add(hand, scores);
            trackers.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.zero);
            Vector3 moveRight = Vector3.right*trackers.WorldBounds().size.x;

            foreach (CardSlot cardSlot in trackers)
            {
                cardSlot.transform.position += moveRight;
            }
            //trackers.PositionAlongLineCentered(Vector3.left, 0.5f, Vector3.right * trackers.WorldBounds().size.x);

            _roundScoresPerHand.Add(hand, trackers);
            
        }

        //hide them.
    }

    
    //These come in the order they were played - will change to list in time
    //todo: score per stack or what?
    internal void AddRound(Stack stack, Card firstCard, Card secondCard)
    {

        Hand firstHand = _roundScoresPerHand.Keys.FirstOrDefault(x => x.PlayerSoRef == firstCard.PlayerSoRef);
        Hand secondHand = _roundScoresPerHand.Keys.FirstOrDefault(x => x.PlayerSoRef == secondCard.PlayerSoRef);

        CardSlot firstSlot = _roundScoresPerHand[firstHand][roundNumber];
        CardSlot secondSlot = _roundScoresPerHand[secondHand][roundNumber];

        firstSlot.AddCardToSlot(firstCard);
        secondSlot.AddCardToSlot(secondCard);

        if (stack.GetTopTokenSide() == firstCard.PlayerSoRef.DesiredTokenSide)
        {
            Debug.Log(firstCard.PlayerSoRef.name + " wins round " + (roundNumber + 1) + " for " + _roundScores[roundNumber] + "point(s)");
            PositionWinningCard(firstCard, firstSlot, firstHand);
        }
        else
        {
            Debug.Log(secondCard.PlayerSoRef.name + " wins round " + (roundNumber+1) + " for " + _roundScores[roundNumber] + "point(s)");
            PositionWinningCard(secondCard, secondSlot, secondHand);
        }


        roundNumber++;
    }

    private void PositionWinningCard(Card firstCard, CardSlot firstSlot, Hand firstHand)
    {
        _scores[firstHand][roundNumber] = _roundScores[roundNumber]; //claim the points

        firstSlot.transform.position =
            firstSlot.transform.position + Vector3.up*0.125f;

        firstSlot.transform.position =
            firstSlot.transform.position +
            -firstSlot.transform.forward*firstCard.transform.RenderBounds().size.z*
            (float) _roundScores[roundNumber]/5f;
    }
}
