using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stack : MonoBehaviour {

    public Token TokenPrefab;
    public float Gap = 0.5f;
    private StackSO _stackSoRef;

    private List<Token> _stackOfTokens;
    private List<CardSO> _listOfOperationsApplied;
    public void Init(StackSO stackSo)
    {
        _listOfOperationsApplied = new List<CardSO>();

        _stackSoRef = stackSo;

        _stackOfTokens = new List<Token>();
        for (int i = 0; i < _stackSoRef.NumberOfTokens; i++)
        {
            Token toke = transform.InstantiateChild<Token>(TokenPrefab);
            _stackOfTokens.Add(toke);
        }
        
       _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f ,Vector3.zero);

    }


    public void ApplyCardToStack(CardSO cardOp)
    {
        if (cardOp.FlipBottom)
        {
            List<Token> topSet = GetBottomGroup();
            topSet.Reverse();

            foreach (Token token in topSet)
            {
                token.Flip();
            }
            
        }

        if (cardOp.FlipTop)
        {
            List<Token> topSet = GetTopGroup();
            topSet.Reverse();

            foreach (Token token in topSet)
            {
                token.Flip();
                _stackOfTokens.Remove(token);//remove from stack
            }
            foreach (Token token in topSet)
            {
                //put back in stack in reversed order
                _stackOfTokens.Add(token);
            }

        }

        if (cardOp.FlipStack)
        {

        }
    }

    private List<Token> GetBottomGroup()
    {
        List<Token> bottomSet = new List<Token>();

        int numTokens = _stackOfTokens.Count;

        for (int i = 0; i < _stackSoRef.NumberOfTokens - 1; i++)
        {
            bottomSet.Add(_stackOfTokens[i]);
        }
        return bottomSet;
    }

    //get all except bottom, basically.
    private List<Token> GetTopGroup()
    {
        List<Token> topSet = new List<Token>();

        int numTokensToFlip = _stackOfTokens.Count-1;

        for (int i = 0; i < numTokensToFlip; i++)
        {
            topSet.Add(_stackOfTokens[_stackOfTokens.Count-1-i]);
        }
        return topSet;
    }

    public void Undo()
    {
        CardSO lastOp = _listOfOperationsApplied.Peek();

        if (lastOp.FlipBottom)
        {

            //QueueFlipAnim
        }

        if (lastOp.FlipTop)
        {
        }

        if (lastOp.FlipStack)
        {

        }
    }
	
}
