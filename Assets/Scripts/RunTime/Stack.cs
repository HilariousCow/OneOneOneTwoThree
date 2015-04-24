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

    private bool _isPreview;
 /*   private int _flipBothHash = Animator.StringToHash("FlipBoth");
    private int _flipTopHash = Animator.StringToHash("FlipTop");
    private int _flipBottomHash = Animator.StringToHash("FlipBottom");
    private int _flipStackHash = Animator.StringToHash("FlipStack");
    private int _flipSwapHash = Animator.StringToHash("Swap");
    private int _flipNothingHash = Animator.StringToHash("Nothing");*/

    public void Init(StackSO stackSo)
    {
        _isPreview = transform.parent.GetComponent<Card>() != null;
        //before creating tokens, hide the temp tokens
        Renderer[] tempTokens = GetComponentsInChildren<Renderer>();
        foreach (Renderer tempToken in tempTokens)
        {
            tempToken.enabled = false;
        }
        _listOfOperationsApplied = new List<CardSO>();

        _stackSoRef = stackSo;

        _stackOfTokens = new List<Token>();
        Transform[] poses = new Transform[] { TopHandle, BottomHandle };
        for (int i = 0; i < _stackSoRef.NumberOfTokens; i++)
        {
            Token toke = poses[i].InstantiateChild<Token>(TokenPrefab);
            toke.gameObject.name = i == 0 ? "StartingTopToken" : "StartingBottomToken";
            _stackOfTokens.Add(toke);
        }
        
       //_stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f ,Vector3.zero);

       // transform.localScale = Vector3.one*2.0f;

        _anim = GetComponent<Animator>();
    }

 
    public IEnumerator AnimateCardEffectOnStack(Card card)
    {
        if (!_isPreview)
        {
            SoundPlayer.Instance.PlaySound(card.CardSoRef.name);
        }
        AnimationClip animClip = card.CardSoRef.StackAnimation;
        if (animClip != null)
        {
            _anim.SetTrigger(animClip.name);
            yield return new WaitForSeconds(animClip.length-Time.deltaTime*5);//wait for a frame so it can get started
          /*  int frameCounter = Mathf.FloorToInt(animClip.frameRate*animClip.length);
            while(frameCounter > 0)
            {
                yield return new WaitForFixedUpdate();
                frameCounter--;
            }*/

            // yield return new WaitForEndOfFrame();
        }

        if (_anim.GetCurrentAnimatorStateInfo(0).IsName(animClip.name))
        {
            Debug.Log("Anim state still playing");
        }
        ApplyCardToStack(card);
    }
   
    public void ApplyCardToStack(Card card)
    {
        
        if(_isPreview)return;
     //   Debug.Break();
        Debug.Log("Applying" + card.name);
        CardSO cardOp = card.CardSoRef;
        
        if (cardOp.FlipBottom)
        {
           /* List<Token> bottomSet = GetBottomGroup();
            bottomSet.Reverse();

            foreach (Token token in bottomSet)
            {
                token.Flip();
            }*/
            _stackOfTokens[1].Flip();
            
        }

        if (cardOp.FlipTop)
        {
           /* List<Token> topSet = GetTopGroup();
            topSet.Reverse();

            foreach (Token token in topSet)
            {
                token.Flip();
            }*/
            _stackOfTokens[0].Flip();

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

        Transform[] poses = new Transform[] {TopHandle, BottomHandle, };
        for (int i = 0; i < _stackOfTokens.Count; i++)
        {
            Token toke = _stackOfTokens[i];
            toke.transform.parent = poses[i];
            //toke.transform.ResetToParent();
            toke.transform.localPosition = Vector3.zero;
            toke.transform.localScale = Vector3.one;
        }

        _listOfOperationsApplied.Add(card.CardSoRef);

      //  _stackOfTokens.PositionAlongLineCentered(Vector3.up, 0.125f, Vector3.zero);

    }

    private List<Token> GetBottomGroup()
    {
        List<Token> bottomSet = new List<Token>();

        int numTokens = _stackOfTokens.Count-1;

        for (int i = 0; i < numTokens - 1; i++)
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
        //void
        if (!cardSoRef.ReverseStack && !cardSoRef.FlipBottom && !cardSoRef.FlipTop && !cardSoRef.FlipStack)
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in rends)
            {
                rend.enabled = false;
            }

        }
        StartCoroutine("LoopAnim",cardSoRef);
        
    }

    internal TokenSide GetTopTokenSide()
    {
        return _stackOfTokens.PeekFront().CurrentSide;
    }

    internal void IdleAnim()
    {
        if (_isPreview) StopCoroutine("LoopAnim");
        _anim.SetTrigger("Idle");
    }



    void LateUpdate()
    {
        if (!_isPreview)
        {
            //always try to move to your home position
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero,
                                                          (transform.localPosition.magnitude + 0.1f)*Time.deltaTime*5f);

            Quaternion targetRot = Quaternion.identity;
            float angle = Quaternion.Angle(transform.localRotation, targetRot);
            if (angle > 0.0f)
            {
                angle += 10.0f;
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot,
                                                                   Time.deltaTime*angle*5f);
            }

            foreach (Token stackOfToken in _stackOfTokens)
            {
                stackOfToken.transform.localPosition = Vector3.MoveTowards(stackOfToken.transform.localPosition, Vector3.zero,
                                                         (stackOfToken.transform.localPosition.magnitude + 0.1f) * Time.deltaTime * 5f);
            }
        }
    }

    public void CopyTokenPositions(int index, Token top)
    {
        _stackOfTokens[index].transform.position = top.transform.position;


        _stackOfTokens[index].transform.rotation = top.transform.rotation;
      

        TokenSide topSide = Vector3.Dot(Vector3.up, top.transform.up) > 0.0f
                             ? TokenSide.White
                             : TokenSide.Black;



        _stackOfTokens[index].SetSide(topSide);


        foreach (Renderer rend in _stackOfTokens[index].GetComponentsInChildren<Renderer>())
        {
            rend.enabled = true;
        }
    }
}
