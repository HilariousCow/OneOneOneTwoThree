using UnityEngine;
using System.Collections;

public class CardSO : ScriptableObject 
{
    public AnimationClip StackAnimation;//maybe not. need to do this in code probably.

    public bool FlipTop;
    public bool FlipBottom;
    public bool FlipStack;
}
