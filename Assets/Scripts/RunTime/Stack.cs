﻿using UnityEngine;
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


    public void ApplyCardToStack(Card card)
    {
        CardSO cardOp = card.CardSoRef;
        
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
            _stackOfTokens.Reverse();
            foreach (Token token in _stackOfTokens)
            {
                token.Flip();
            }
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

    //this is an update rather than a nice trigger, bleurgh
    //i just want to be able to tell which is which from a glance
    internal void PlayOperationAnimation(CardSO _cardSoRef)
    {

        float centerOfScreen = Mathf.Clamp01(Vector3.Dot(Camera.main.transform.forward,
                                           (transform.position - Camera.main.transform.position).normalized));
        float rotateSpeed = Mathf.SmoothStep(0.0f, 360.0f, centerOfScreen * centerOfScreen);
        //todo: this should be done in the stack. get IT to trigger animations.

        //flip stack
        if (_cardSoRef.FlipStack && !_cardSoRef.FlipBottom && !_cardSoRef.FlipTop)
        {
            transform.localRotation = transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * rotateSpeed, Vector3.forward);
        }

        //swap
        if (_cardSoRef.FlipStack && _cardSoRef.FlipBottom && _cardSoRef.FlipTop)
        {
            transform.localRotation = transform.localRotation *
                                                    Quaternion.AngleAxis(-Time.deltaTime * rotateSpeed, Vector3.forward);
            for (int index = 0; index < _stackOfTokens.Count; index++)
            {
                //if (index != 0 || index != _stackOfTokens.Count - 1) continue;
                
                
                Token token = _stackOfTokens[index];
                float fraction = index/(float) _stackOfTokens.Count;

                Vector3 pos = Vector3.zero;
                pos = pos.SinCosZ(Time.deltaTime + fraction );
                pos *= token.transform.localPosition.magnitude;
                token.transform.localPosition = pos;


                _stackOfTokens[index].transform.localRotation = _stackOfTokens[index].transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * rotateSpeed, Vector3.forward);
                _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.25f, Vector3.zero);

            }
        }


        if (_cardSoRef.FlipTop && !_cardSoRef.FlipStack)
        {
            _stackOfTokens[_stackOfTokens.Count - 1].transform.localRotation = _stackOfTokens[_stackOfTokens.Count - 1].transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * rotateSpeed, Vector3.forward);
            _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.25f, Vector3.zero);
        }
        if (_cardSoRef.FlipBottom && !_cardSoRef.FlipStack)
        {
            _stackOfTokens[0].transform.localRotation = _stackOfTokens[0].transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * rotateSpeed, Vector3.forward);
            _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.25f, Vector3.zero);
        }

        //void
        if (!_cardSoRef.FlipStack && !_cardSoRef.FlipBottom && !_cardSoRef.FlipTop)
        {
            transform.localScale = Vector3.zero;
        }
    }

    internal TokenSide GetTopTokenSide()
    {
        return _stackOfTokens.Peek().CurrentSide;
    }
}
