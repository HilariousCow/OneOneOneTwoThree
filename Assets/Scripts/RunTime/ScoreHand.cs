using UnityEngine;
using System.Collections;

public class ScoreHand : MonoBehaviour {

    internal void Init(System.Collections.Generic.List<Hand> _hands, MatchSettingsSO _matchSettings)
    {
       
    }

    

    internal void AddRound(Stack stack, Card firstCard, Card secondCard)
    {
        Destroy(firstCard.gameObject);
        Destroy(secondCard. gameObject);
    }
}
