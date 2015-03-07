using UnityEngine;
using System.Collections;

public class Stack : MonoBehaviour {

    public Token TokenPrefab;
    private StackSO _stackSoRef;

    public void Init(StackSO stackSo)
    {
        _stackSoRef = stackSo;
    }

    public void ApplyCardToStack(Card card)
    {

    }

	
}
