using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stack : MonoBehaviour
{

    public Transform TopHandle;
    public Transform BottomHandle;

    public Token TokenPrefab;

    private StackSO _stackSoRef;

    private List<Token> _stackOfTokens;//0 = top, count-1 = bottom
    private List<CardSO> _listOfOperationsApplied;
    private Animator _anim;

 /*   private int _flipBothHash = Animator.StringToHash("FlipBoth");
    private int _flipTopHash = Animator.StringToHash("FlipTop");
    private int _flipBottomHash = Animator.StringToHash("FlipBottom");
    private int _flipStackHash = Animator.StringToHash("FlipStack");
    private int _flipSwapHash = Animator.StringToHash("Swap");
    private int _flipNothingHash = Animator.StringToHash("Nothing");*/

    public void Init(StackSO stackSo)
    {
        _listOfOperationsApplied = new List<CardSO>();

        _stackSoRef = stackSo;

        _stackOfTokens = new List<Token>();
        Transform[] poses = new Transform[] { TopHandle , BottomHandle};
        for (int i = 0; i < _stackSoRef.NumberOfTokens; i++)
        {
            Token toke = poses[i].InstantiateChild<Token>(TokenPrefab);
            _stackOfTokens.Add(toke);
        }
        
       //_stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f ,Vector3.zero);

       // transform.localScale = Vector3.one*2.0f;

        _anim = GetComponent<Animator>();
    }

    public IEnumerator AnimateCardEffectOnStack(Card card)
    {
        //do animation stuff, but then reset and do the actual code change at the end.
        Quaternion prevRot = transform.rotation;
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, -card.transform.position);

        

        yield return new WaitForSeconds(0.5f);
        
        AnimationClip animClip = card.CardSoRef.StackAnimation;
        if (animClip != null)
        {
            _anim.SetTrigger(animClip.name);
            yield return new WaitForSeconds(animClip.length);
        }
        //get the animation and wait for the length of that animation



        transform.rotation = prevRot;
        ApplyCardToStack(card);
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, -card.transform.position);

        yield return new WaitForSeconds(0.5f);
        transform.rotation = prevRot;
    }

    public void ApplyCardToStack(Card card)
    {
       
        CardSO cardOp = card.CardSoRef;
        
        if (cardOp.FlipBottom)
        {
            List<Token> bottomSet = GetBottomGroup();
            bottomSet.Reverse();

            foreach (Token token in bottomSet)
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

        //i think i do need a swap! It's a reverse without flips

        if (cardOp.ReverseStack)
        {
            _stackOfTokens.Reverse();
        }


        if (cardOp.FlipStack)
        {
            _stackOfTokens.Reverse();
            foreach (Token token in _stackOfTokens)
            {
                token.Flip();
            }
        }
        _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f, Vector3.zero);

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

        if (lastOp.ReverseStack)
        {

        }
    }

    IEnumerator LoopAnim(CardSO cardSoRef)
    {
        AnimationClip animClip = cardSoRef.StackAnimation;
        if (animClip != null)
        {
            while (true)
            {
                _anim.SetTrigger(animClip.name);
                yield return new WaitForSeconds(animClip.length);
            }
        }
    }
    //this is an update rather than a nice trigger, bleurgh
    //i just want to be able to tell which is which from a glance
    internal void PlayOperationLooping(CardSO cardSoRef)
    {

        StartCoroutine(LoopAnim(cardSoRef));

        //void
        if (!cardSoRef.ReverseStack && !cardSoRef.FlipBottom && !cardSoRef.FlipTop && !cardSoRef.FlipStack)
        {
            transform.localScale = Vector3.zero;
        }

        return;
        float centerOfScreen = Mathf.Clamp01(Vector3.Dot(Camera.main.transform.forward,
                                           (transform.position - Camera.main.transform.position).normalized));
        float rotateSpeed = 180.0f;
        float cosSpeed = rotateSpeed * (Mathf.Cos(Time.time*Mathf.PI) + 1.0f) * 0.5f;
        float sinSpeed =  rotateSpeed * (Mathf.Sin(Time.time*Mathf.PI) + 1.0f) * 0.5f;

        //todo: this should be done in the stack. get IT to trigger animations.

        //flip stack
        if (cardSoRef.FlipStack)
        {
            transform.localRotation = transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * sinSpeed, Vector3.forward);
            //_stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.25f, Vector3.zero);    
        }

        //swap
        if (cardSoRef.ReverseStack)
        {

            Vector3 dir = Quaternion.AngleAxis(Time.time * rotateSpeed , Vector3.forward) * Vector3.right;

            List<Token> _flippedOrNot = new List<Token>(_stackOfTokens);
           
            _flippedOrNot.PositionAlongLineCentered(dir, 0.0f, Vector3.zero);
        }


        if (cardSoRef.FlipTop )
        {
            _stackOfTokens[_stackOfTokens.Count - 1].transform.localRotation = _stackOfTokens[_stackOfTokens.Count - 1].transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * sinSpeed, Vector3.forward);
            _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f, Vector3.zero);
        }
        if (cardSoRef.FlipBottom )
        {
            _stackOfTokens[0].transform.localRotation = _stackOfTokens[0].transform.localRotation *
                                                    Quaternion.AngleAxis(Time.deltaTime * sinSpeed, Vector3.forward);
            _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f, Vector3.zero);    
        }

        //void
        if (!cardSoRef.ReverseStack && !cardSoRef.FlipBottom && !cardSoRef.FlipTop && !cardSoRef.FlipStack)
        {
            transform.localScale = Vector3.zero;
        }

        
    }

    internal TokenSide GetTopTokenSide()
    {
        return _stackOfTokens.Peek().CurrentSide;
    }
}
