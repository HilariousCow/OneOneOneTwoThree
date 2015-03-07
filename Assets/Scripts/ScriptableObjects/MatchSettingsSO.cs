using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class MatchSettingsSO : ScriptableObject
{

    public int NumberOfStacks = 1;
    public StackSO StackStyle;
   
    public List<CardSO> CardsPerHand;

    public List<PlayerSO> Players; 
}
