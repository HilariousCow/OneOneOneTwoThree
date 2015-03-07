using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class MatchSettingsSO : ScriptableObject
{
    public List<StackSO> Stacks;
   
    public List<CardSO> CardsPerHand;

    public List<PlayerSO> Players; 
}
