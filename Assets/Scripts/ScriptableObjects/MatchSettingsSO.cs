using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public enum TieBreakerStyle
{
    FlipStack,
    UseJailCards,
    GoldenGoal //just play the extra card
}

public class MatchSettingsSO : ScriptableObject
{

    public int NumberOfRounds = 5;
    public int[] RoundScores = new int[] {1, 1, 1, 2, 3};
    public StackSO StackStyle;
   
    public List<CardSO> CardsPerHand;

    public List<PlayerSO> Players;


    public TieBreakerStyle TieBreaker;

    public CardSO TieBreakerCard;
    public PlayerSO TieBreakerPlayer;//just necessary to spawn a card


    
}
