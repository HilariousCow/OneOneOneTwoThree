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
       /*     List<KeyValuePair<Hand, int>> sortedDict = (from entry in TotalledScores orderby entry.Value descending select entry).ToList();
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

            return numTiedForWin > 1;*/

            int score = -1;
            foreach (KeyValuePair<Hand, int> totalledScore in TotalledScores)
            {
                if(score == -1)
                {
                    score = totalledScore.Value;
                }

                if(score!= totalledScore.Value)
                {
                    return false;
                }
            }
            return true;

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
                slot.IsInteractive = false;//never interactive
                trackers.Add(slot);

                scores.Add(0);
            }
            _scores.Add(hand, scores);
            trackers.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.zero);
            /*Vector3 moveRight = Vector3.right*trackers.WorldBounds().size.x;

            foreach (CardSlot cardSlot in trackers)
            {
                cardSlot.transform.position += moveRight;
            }*/
            //trackers.PositionAlongLineCentered(Vector3.left, 0.5f, Vector3.right * trackers.WorldBounds().size.x);

            _roundScoresPerHand.Add(hand, trackers);
            
        }

        //hide them.
    }

    
    //These come in the order they were played - will change to list in time
    //todo: score per stack or what?
    internal IEnumerator AddRound(Stack stack, CardSlot firstCommitCardslot, CardSlot secondCommitCardSlot)
    {
        Card firstCard = firstCommitCardslot.CardInSlot;
        Card secondCard = secondCommitCardSlot.CardInSlot;

        Hand firstHand = _roundScoresPerHand.Keys.FirstOrDefault(x => x.PlayerSoRef == firstCard.PlayerSoRef);
        Hand secondHand = _roundScoresPerHand.Keys.FirstOrDefault(x => x.PlayerSoRef == secondCard.PlayerSoRef);

        CardSlot firstSlot = _roundScoresPerHand[firstHand][roundNumber];
        CardSlot secondSlot = _roundScoresPerHand[secondHand][roundNumber];

        

        firstSlot.AddCardToSlot(firstCommitCardslot.RemoveCardFromSlot());
        yield return new WaitForSeconds(0.6f);
        secondSlot.AddCardToSlot(secondCommitCardSlot.RemoveCardFromSlot());

        if (stack.GetTopTokenSide() == firstCard.PlayerSoRef.DesiredTokenSide)
        {

            Debug.Log(firstCard.PlayerSoRef.name + " wins round " + (roundNumber + 1) + " for " + _roundScores[roundNumber] + "point(s)");
            PositionWinningCard(firstCard, firstSlot, firstHand);

        }
        else
        {

            Debug.Log(secondCard.PlayerSoRef.name + " wins round " + (roundNumber + 1) + " for " + _roundScores[roundNumber] + "point(s)");
            PositionWinningCard(secondCard, secondSlot, secondHand);
        }

        //"Black wins 1 point"
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(stack.GetTopTokenSide().ToString()));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("Wins"));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(_roundScores[roundNumber].ToString() + " point"));
        //flash a score point here or something?

        if (roundNumber > 0)
        {
            yield return new WaitForSeconds(1.6f);
            yield return StartCoroutine(TellMeTheScores());
        }


        roundNumber++;
    }
    public IEnumerator AnnouceRoundNumber()
    {
        SoundPlayer.Instance.PlaySound("Round");
        yield return new WaitForSeconds(0.6f);
        SoundPlayer.Instance.PlaySound((roundNumber+1).ToString());
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator TellMeTheScores()
    {
        

        foreach (KeyValuePair<Hand, List<int>> keyValuePair in _scores)
        {
            TokenSide side = keyValuePair.Key.PlayerSoRef.DesiredTokenSide;
            SoundPlayer.Instance.PlaySound(side.ToString());

            yield return new WaitForSeconds(0.6f);
            SoundPlayer.Instance.PlaySound(TotalledScores[keyValuePair.Key].ToString());

            yield return new WaitForSeconds(0.6f);
        }
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
