using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreHand : MonoBehaviour
{
    public Round RoundPrefab;
    private List<Round> _rounds;
    private List<Hand> _hands;
    private int _roundNumber = 0;
    private int _numberOfRounds = 5;

    public bool FinishedAllRounds { get { return _roundNumber == _rounds.Count; } }
    public bool GameIsATie { get
        {
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

            foreach (Hand hand in _hands)
            {
                scores.Add(hand, 0);
                foreach (Round round in _rounds)
                {
                   scores[hand] += round.GetScoreForHand(hand);
                }
            }

            return scores;
        }
    }
  
    internal void Init(List<Hand> hands, MatchSettingsSO _matchSettings)
    {
        _roundNumber = 0;
        _numberOfRounds = _matchSettings.NumberOfRounds;
        _hands = hands;
        _rounds = new List<Round>();
        for (int i = 0; i < _numberOfRounds; i++)
        {
            Round round = transform.InstantiateChild<Round>(RoundPrefab);
            round.Init(hands, i, _matchSettings);
            _rounds.Add(round);
        }

        _rounds.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.zero);
     
    }

    
    //These come in the order they were played - will change to list in time
    //todo: score per stack or what?
    internal IEnumerator RoundResolution(Stack stack, CardSlot firstPlayedSlot, CardSlot secondPlayedSlot)
    {

        Round currentRound = _rounds[_roundNumber];

        TokenSide winningSide = stack.GetTopTokenSide();

        List<Card> cardsPlayed = new List<Card>(new Card[]{firstPlayedSlot.CardInSlot,secondPlayedSlot.CardInSlot});

        Card winningCard = cardsPlayed.Find(x => x.PlayerSoRef.DesiredTokenSide == winningSide);
        Card losingCard = cardsPlayed.Find(x => x.PlayerSoRef.DesiredTokenSide != winningSide);

        currentRound.ResolveRound(winningCard, losingCard);
        Debug.Log(winningCard.PlayerSoRef.name + " wins round " + (_roundNumber + 1) + " for " + currentRound.RoundValue + "point(s)");
          
        //"Black wins 1 point"
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(stack.GetTopTokenSide().ToString()));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("Wins"));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(currentRound.RoundValue.ToString() + " point"));
        //flash a score point here or something?

        if (_roundNumber > 0)
        {
            yield return new WaitForSeconds(1.6f);
            yield return StartCoroutine(TellMeTheScores());
        }


        _roundNumber++;
    }
    public IEnumerator AnnouceRoundNumber()
    {
        SoundPlayer.Instance.PlaySound("Round");
        yield return new WaitForSeconds(0.6f);
        SoundPlayer.Instance.PlaySound((_roundNumber+1).ToString());
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator TellMeTheScores()
    {
        foreach (Hand hand in _hands)
        {
            TokenSide side = hand.PlayerSoRef.DesiredTokenSide;
            SoundPlayer.Instance.PlaySound(side.ToString());
            yield return new WaitForSeconds(0.6f);
            SoundPlayer.Instance.PlaySound(TotalledScores[hand].ToString());

            yield return new WaitForSeconds(0.6f);
        }
    }


    void LateUpdate()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero,
                                                       (transform.localPosition.magnitude + 0.1f) * Time.deltaTime * 5f);

        Quaternion targetRot = Quaternion.identity;
        float angle = Quaternion.Angle(transform.localRotation, targetRot);
        if (angle > 0.0f)
        {
            angle += 10.0f;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot,
                                                               Time.deltaTime * angle * 5f);
        }
    }

    internal void StartNewRound()
    {
        //get round slot for round number
        
    }
}
