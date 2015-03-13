using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreHand : MonoBehaviour
{
    public CardSlot CardSlotScoreKeepingPrefab;


    public bool FinishedRound { get { return roundNumber == numberOfRounds - 1; } }
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
            trackers.PositionAlongLineCentered(Vector3.left, 0.125f, Vector3.left * trackers.WorldBounds().size.x);

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


        _roundScoresPerHand[firstHand][roundNumber].AddCardToSlot(firstCard);
        _roundScoresPerHand[secondHand][roundNumber].AddCardToSlot(secondCard);

        if (stack.GetTopTokenSide() == firstCard.PlayerSoRef.DesiredTokenSide)
        {
            
            _scores[firstHand][roundNumber] = _roundScores[roundNumber];//claim the points

            firstCard.transform.localPosition = firstCard.transform.localPosition + Vector3.up * 0.125f;
            //move card to reveal dots below
            firstCard.transform.localPosition = firstCard.transform.localPosition +
                                                Vector3.back*firstCard.transform.RenderBounds().size.z *
                                                (float) _roundScores[roundNumber]/5f;

        }
        else
        {
            secondCard.transform.localPosition = secondCard.transform.localPosition + Vector3.up*0.125f;
            _scores[secondHand][roundNumber] = _roundScores[roundNumber];//claim the points
            secondCard.transform.localPosition = secondCard.transform.localPosition +
                                                Vector3.back * secondCard.transform.RenderBounds().size.z *
                                                (float)_roundScores[roundNumber] / 5f;
        }


        roundNumber++;
    }
}
