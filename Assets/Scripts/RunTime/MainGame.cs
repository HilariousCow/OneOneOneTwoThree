using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public struct GameState
{
    public Stack StackState;
    public Hand BlackPlayer;
    public Hand WhitePlayer;
}

//override for different game rules.
public class MainGame : MonoBehaviour, IDropCardOnCardSlot, IPointerClickOnCard
{
    public MatchSettingsSO MatchToUseDefault;

    public AIPlayer AIPrefab;
    public Hand HandPrefab;//hands are effectively "the player"
    public Card CardPrefab;
    public Stack StackPrefab;
    public CardSlot CardSlotPrefab;
    public ScoreHand ScoreHandPrefab;
    public Transform StackHandle;

    public Token DropTokens;
    public Collider DropTable;

    public FollowTransform CameraRigRoot;
    public LookAtTransform CameraMain;
    internal Stack MainStack;
    //public TextMesh TestMeshPrefab;

    //need cards sos
    private MatchSettingsSO _matchSettings;
    private List<Hand> _hands;
    private List<Card> _cards;//all cards
    private List<Stack> _stacks;//all cards
    private ScoreHand _scoreHand;

    private Dictionary<Hand, List<CardSlot>> _handsToJailCards;//not always used
    private Dictionary<Hand,List<CardSlot>> _handsToCommitSlots;
    private Dictionary<CardSlot, Hand> _slotsToHands;

    private Dictionary<Stack, List<CardSlot>> _stacksToCommitSlots;
    private List<CardSlot> _allCommitCardSlots;
    private List<CardSlot> _allJailCardSlots;


    public AIPlayer _whitePlayer;
    public AIPlayer _blackPlayer;

    void Awake()
    {
        AIPlayer whitePlayer = null;// transform.InstantiateChild<AIPlayer>(AIPrefab);
        
        AIPlayer blackPlayer = null;
        Init(MatchToUseDefault, whitePlayer, blackPlayer);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        StartCoroutine("Toss");
        
    }
    IEnumerator Toss()
    {

        TurnOffScoreHands();
        TurnOffJailSlotInteractivity();
        TurnOffCommitSlotInteractivity();
        TurnOffHands();


        yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine("OneOneOneTwoThree"));
        yield return new WaitForSeconds(0.5f);
        SoundPlayer.Instance.PlaySound("Drop the Stack");

        Time.timeScale = 2.0f;
        Time.fixedDeltaTime =1f/30f;
        Token tokenA = transform.InstantiateChild(DropTokens);
        tokenA.transform.position = Vector3.up * 20f + UnityEngine.Random.onUnitSphere.FlatY().normalized;
        tokenA.transform.rotation = UnityEngine.Random.rotation;
        tokenA.rigidbody.AddForce(Vector3.down, ForceMode.VelocityChange);
        tokenA.rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles * 2f, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.15f);
        Token tokenB = transform.InstantiateChild(DropTokens);
        tokenB.transform.position = Vector3.up * 30f + UnityEngine.Random.onUnitSphere.FlatY().normalized;
        tokenB.transform.rotation = UnityEngine.Random.rotation;
        tokenB.rigidbody.AddForce(Vector3.down, ForceMode.VelocityChange);
        tokenB.rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles*2f, ForceMode.VelocityChange);

        CameraRigRoot.SetTarget(tokenB.transform);
        foreach (Renderer  rend in StackHandle.GetComponentsInChildren<Renderer>())
        {
            rend.enabled = false;
        }

        float sleepVel = 0.1f;
        Token lastToStopMoving = tokenA;
        Token other = tokenB;
        yield return new WaitForFixedUpdate();
        while (tokenA.rigidbody.velocity.magnitude > sleepVel || tokenB.rigidbody.velocity.magnitude > sleepVel)
        {
            if (tokenA.rigidbody.velocity.magnitude > sleepVel && tokenB.rigidbody.velocity.magnitude <= sleepVel)
            {
                lastToStopMoving = tokenA;
                other = tokenB;
            }

            if (tokenA.rigidbody.velocity.magnitude <= sleepVel && tokenB.rigidbody.velocity.magnitude > sleepVel)
            {
                lastToStopMoving = tokenB;
                other = tokenA;
            }

            yield return new WaitForFixedUpdate();
        }
        //last to stop moving?
        
        Debug.Log("Stopped moving");
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 1f / 60f;

        CameraRigRoot.SetTarget(null);
        yield return new WaitForSeconds(0.5f);
        _stacks[0].CopyTokenPositions(1, other);

        Destroy(other.gameObject);
        yield return new WaitForSeconds(0.5f);


        _stacks[0].CopyTokenPositions(0, lastToStopMoving);
        Destroy(lastToStopMoving.gameObject);
        Destroy(DropTable.gameObject);
        yield return new WaitForSeconds(2.5f);
     


        StartCoroutine("IntroPhase");
    }

    
    IEnumerator IntroPhase()
    {
        //randomize stack
        yield return null;
        //see if we need the tie breaker intro
        switch (_matchSettings.TieBreaker)
        {

            case TieBreakerStyle.FlipStack:
                TurnOffCommitSlotInteractivity();
                break;
            case TieBreakerStyle.UseJailCards:
                TurnOffCommitSlotInteractivity();
                TurnOnJailSlotInteractivity();
                SetAllHandsToJailCard();

                yield return new WaitForSeconds(1f);
                TurnOnHands();
                SoundPlayer.Instance.PlaySound("PickReserve");
                bool all = (_allJailCardSlots.FindAll(x => !x.IsEmpty).Count == _allJailCardSlots.Count);
                bool pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;

                ProdAIForMove();
                while (!all || !pointDownMosty)
                {

                    yield return null;
                    pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
                    all = (_allJailCardSlots.FindAll(x => !x.IsEmpty).Count == _allJailCardSlots.Count);
                }
                TurnOffJailSlotInteractivity();
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(MoveJailCardsUnderStack());
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        StartCoroutine(LoopPhase());
       
    }

    private void ProdAIForMove()
    {
        GameState state = GetCurrentGameState();
        if (_whitePlayer != null)
        {

            _whitePlayer.ChooseCardForGameState(state);
            _whitePlayer.CardChosen += CardChosen;
        }
        if (_blackPlayer != null)
        {
            _blackPlayer.ChooseCardForGameState(state);
            _blackPlayer.CardChosen += CardChosen;
        }

    }

    private void CardChosen(CardSlot cardSlot, AIPlayer ai)
    {
        ai.CardChosen -= CardChosen;

        cardSlot.OnCardSlotClick();
        //pretend our ai friends clicked on this
    }

    private GameState GetCurrentGameState()
    {
        GameState state = new GameState();
        state.WhitePlayer = _hands[0];
        state.BlackPlayer = _hands[1];
        state.StackState = _stacks[0];

        return state;//todo: previous plays in order by both.
    }

    private IEnumerator MoveJailCardsUnderStack()
    {
        TokenSide topAtBeginningOfOperation = _stacks[0].GetTopTokenSide();

        foreach (Hand hand in _hands)
        {
            
            CardSlot jailslot = _handsToJailCards[hand][0];
            Card tieBreakerCard = jailslot.RemoveCardFromSlot();
            CameraMain.SetTarget(jailslot.transform);

            jailslot.transform.position = _stacks[0].transform.position + transform.right * -9f;
            jailslot.transform.rotation = Quaternion.LookRotation(transform.right, Vector3.up);
            if (topAtBeginningOfOperation == hand.PlayerSoRef.DesiredTokenSide)
            {

                jailslot.transform.position += Vector3.down * 2.0f;
            }
            else
            {
                jailslot.transform.position += Vector3.down * 2.50f;
            }

            jailslot.AddCardToSlot(tieBreakerCard);
            
        }
        yield return new WaitForSeconds(1f);

    }

    private void SetAllHandsToJailCard()
    {
        foreach (Hand hand in _hands)
        {
            CardSlot jailSlot = _handsToJailCards[hand][0];
            hand.AutoPlacementTargetSlot = jailSlot;
            

        }
    }

    private void SetAllHandsToCommitSlot()
    {
        foreach (Hand hand in _hands)
        {
            CardSlot targetSlot = _handsToCommitSlots[hand][0];
            hand.AutoPlacementTargetSlot = targetSlot;
        }

    }

    IEnumerator LoopPhase()
    {
        CameraMain.SetTarget(_stacks[0].transform);
        CameraRigRoot.SetTarget(null);
        yield return StartCoroutine(_scoreHand.AnnouceRoundNumber());


        
        TurnOnHands();
        ClearEmptySlotsInHands();

        TurnOnCommitSlotInteractivity();
        SetAllHandsToCommitSlot();
        TokenSide topAtBeginningOfOperation = _stacks[0].GetTopTokenSide();

        yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine(topAtBeginningOfOperation.ToString()));
        yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine("GoFirst"));

        foreach (Stack stack in _stacks)
        {
            //find whose is applied first for this stack

            List<CardSlot> slotsForStack = new List<CardSlot>(_stacksToCommitSlots[stack]);

            //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
            //so, big assumptions here.
            CardSlot firstCardSlot =
                slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide == topAtBeginningOfOperation);
          
            firstCardSlot.StartNextTurnArrowEffect();
            CardSlot secondCardSlot =
                slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide != topAtBeginningOfOperation);
            secondCardSlot.StopNextTurnArrowEffect();
        }
        //show first slot to apply graphic.

        Debug.LogWarning("Watching for all slots to be filled");
        bool all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);
        bool pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
        ProdAIForMove();
        while (!all || !pointDownMosty)
        {

            yield return null;
            pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
            all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);
        }

        TurnOffCommitSlotInteractivity();
        TurnOffHands();
        Debug.LogWarning("All slots filled, resolving gameplay");
        //todo: lockout changes. focus camera, or have it above in the first place.
        yield return new WaitForSeconds(1.5f);

        //todo: if both stacks are not the same on top, send back the cards.

        foreach (Stack stack in _stacks)
        {
            //find whose is applied first for this stack
            
            List<CardSlot> slotsForStack = new List<CardSlot>(_stacksToCommitSlots[stack] );

            //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
            //so, big assumptions here.
            CardSlot firstCardSlot = slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide == topAtBeginningOfOperation);
            CardSlot secondCardSlot = slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide != topAtBeginningOfOperation);


            //first to go
            CameraMain.SetTarget(_stacks[0].transform);
            CameraRigRoot.SetTarget(_slotsToHands[firstCardSlot].transform);

            firstCardSlot.StartNextTurnArrowEffect();
            yield return StartCoroutine(ApplyCardToStack(firstCardSlot, stack));
            firstCardSlot.StopNextTurnArrowEffect();




            CameraMain.SetTarget(_stacks[0].transform);
            CameraRigRoot.SetTarget(_slotsToHands[secondCardSlot].transform);

            secondCardSlot.StartNextTurnArrowEffect();
            yield return StartCoroutine(ApplyCardToStack(secondCardSlot, stack));
            secondCardSlot.StopNextTurnArrowEffect();


            //moving to score slots
            CameraRigRoot.SetTarget(_scoreHand.transform);
            CameraMain.SetTarget(_scoreHand.transform);
            TurnOnScoreHands();
    
            yield return StartCoroutine(_scoreHand.AddRound(stack, firstCardSlot, secondCardSlot));
            CameraRigRoot.SetTarget(_stacks[0].transform);
            yield return new WaitForSeconds(0.5f);


            CameraMain.SetTarget(_stacks[0].transform);
        }
        

        if (_scoreHand.FinishedRound )
        {
            Debug.Log("Rounds are over");
            StartCoroutine(ResolutionPhase());
        }
        else
        {
            Debug.Log("Starting new round");
            StartCoroutine("LoopPhase");//go again.    
        }

    }

    private void ClearEmptySlotsInHands()
    {
        foreach (Hand hand in _hands)
        {
            hand.ClearOutEmptySlotsAndReorganize();
        }
    }

    

    private void TurnOffHands()
    {
        foreach (Hand hand in _hands)
        {
            hand.gameObject.SetActive(false);
        }
    }


    private void TurnOnHands()
    {
        foreach (Hand hand in _hands)
        {
            hand.gameObject.SetActive(true);
        }
    }

    IEnumerator ApplyCardToStack( CardSlot firstCardSlot, Stack stack)
    {
        
        Card firstCard = firstCardSlot.CardInSlot;
    //    Debug.Log("Showing first card" + firstCard.gameObject.name + " from slot: " + firstCardSlot.gameObject.name);
        firstCard.IdleAnim();
        firstCard.transform.parent = null;
        firstCardSlot.transform.position += Vector3.up*10f;
        firstCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, Vector3.right);
        //firstCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.right);
        firstCard.transform.parent = firstCardSlot.transform;
        
        yield return StartCoroutine(firstCard.PreviewStack.AnimateCardEffectOnStack(firstCard));
        firstCard.IdleAnim();
        yield return new WaitForSeconds(0.5f);//show top for 0.5
        //move stack to new target pos
      
        stack.transform.parent = null;
        stack.transform.rotation = Quaternion.LookRotation(-firstCard.transform.position.FlatY(), Vector3.up);

        StackHandle.transform.position = firstCard.PreviewStack.transform.position + Vector3.up * 3f;
        StackHandle.transform.rotation = firstCard.PreviewStack.transform.rotation;
   
        stack.transform.parent = StackHandle;
        yield return new WaitForSeconds(1.0f);//allow it to get there

        StartCoroutine(firstCard.PreviewStack.AnimateCardEffectOnStack(firstCard));
        yield return StartCoroutine(stack.AnimateCardEffectOnStack(firstCard));
      
        yield return new WaitForSeconds(1.0f);//show top for 0.5

        stack.transform.parent = null;

        StackHandle.transform.position = Vector3.zero;
        StackHandle.transform.rotation = Quaternion.LookRotation(-firstCard.transform.position.FlatY(), Vector3.up);

        stack.transform.parent = StackHandle;
        firstCard.transform.parent = null;
        firstCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, Vector3.right);
        firstCardSlot.transform.position -= Vector3.up * 10f;
        //firstCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.right);
        firstCard.transform.parent = firstCardSlot.transform; 
        yield return new WaitForSeconds(0.25f);//show top for 0.5
    }


    private void TurnOffScoreHands()
    {
        _scoreHand.gameObject.SetActive(false);
    }

    private void TurnOnScoreHands()
    {
        _scoreHand.gameObject.SetActive(true);
    }

    private void TurnOnCommitSlotInteractivity()
    {
        Debug.Log("Turn on commit slots");
        foreach (CardSlot slot in _allCommitCardSlots)
        {
            slot.ShowSlot(true);
            slot.HighlightIfEmpty(true);
            slot.IsInteractive = true;
        }
    }

    private void TurnOffCommitSlotInteractivity()
    {
        Debug.Log("Turn off commit slots");
        foreach (CardSlot slot in _allCommitCardSlots)
        {
            slot.ShowSlot(false);
            slot.HighlightIfEmpty(false);
            slot.IsInteractive = false;
        }
    }

    private void TurnOnJailSlotInteractivity()
    {
        Debug.Log("Turn on jail slots");
        foreach (CardSlot slot in _allJailCardSlots)
        {
            slot.ShowSlot(true);
            slot.HighlightIfEmpty(true);
            slot.IsInteractive = true;
        }
    }

    private void TurnOffJailSlotInteractivity()
    {
        Debug.Log("Turn off jail slots");
        foreach (CardSlot slot in _allJailCardSlots)
        {
            slot.ShowSlot(false);
            slot.HighlightIfEmpty(false);
            slot.IsInteractive = false;
        }
    }
    
    IEnumerator ResolutionPhase()
    {
        TurnOffHands();
        yield return new WaitForSeconds(1f);
        if(_scoreHand.GameIsATie )
        {
            Debug.Log("Game is a draw");
            yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine("TieGame"));
            yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine("OneOneOneTwoThree"));
            if (_matchSettings.TieBreaker == TieBreakerStyle.FlipStack)
            {
                yield return StartCoroutine(TieBreakFilp());
            }
            else if (_matchSettings.TieBreaker == TieBreakerStyle.UseJailCards)
            {
                yield return StartCoroutine(TieBreak());
            }
        }
        else
        {
            List<KeyValuePair<Hand, int>> sortedDict = (from entry in _scoreHand.TotalledScores orderby entry.Value descending select entry).ToList();
            yield return StartCoroutine(DeclareWinner(sortedDict.PeekFront().Key.PlayerSoRef.DesiredTokenSide));
            

        }
    }

    private IEnumerator TieBreakFilp()
    {
        yield return null;
        Debug.Log("Resolving with flip stack");

        List<TokenSide> sides = new List<TokenSide>();
        foreach (Stack stack in _stacks)
        {
            Card tieBreaker = transform.InstantiateChild(CardPrefab);
            tieBreaker.Init(_matchSettings.TieBreakerCard, _matchSettings.StackStyle,
                            _matchSettings.TieBreakerPlayer);
            yield return new WaitForSeconds(0.5f);
            tieBreaker.transform.position = Vector3.up * 2f;
            yield return new WaitForSeconds(0.5f);
            tieBreaker.transform.rotation = tieBreaker.transform.rotation *
                                            Quaternion.AngleAxis(180f, Vector3.forward);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(stack.AnimateCardEffectOnStack(tieBreaker));
            //  stack.ApplyCardToStack(firstCard);..temp remove
            yield return new WaitForSeconds(0.5f);

            Destroy(tieBreaker.gameObject);
            //find whose is applied first for this stack
            sides.Add(stack.GetTopTokenSide());

        }

        yield return new WaitForSeconds(1f);

        //are all sides the same?
        TokenSide firstSide = sides.PeekFront();
        if (sides.Count(x => x == firstSide) == sides.Count)
        {

            StartCoroutine(DeclareWinner(firstSide));
        }
        else
        {
            //now what?
            Debug.LogError("Don't know how to resolve tie for multi stack games");

            yield return StartCoroutine(TieBreakFilp());
        }
    }

    private IEnumerator TieBreak()
    {
        Debug.Log("Doing tie breaker cards.");
        yield return null;

        TokenSide topAtBeginningOfOperation = _stacks[0].GetTopTokenSide();
        List<TokenSide> sides = new List<TokenSide>();
        foreach (Stack stack in _stacks)
        {
            //find whose is applied first for this stack
            List<CardSlot> firstJailSlots =
                _handsToJailCards[_hands.Find(x => x.PlayerSoRef.DesiredTokenSide == topAtBeginningOfOperation)];

            List<CardSlot> secondJailSlots =
                _handsToJailCards[_hands.Find(x => x.PlayerSoRef.DesiredTokenSide != topAtBeginningOfOperation)];

            //reposition
            for (int index = 0; index < secondJailSlots.Count; index++)
            {
                {
                    CardSlot firstJailSlot = firstJailSlots[index];
                    Card firstCard = firstJailSlot.RemoveCardFromSlot();
                    CardSlot firstCommitSlot = _handsToCommitSlots[_slotsToHands[firstJailSlot]][0];

                    firstJailSlot.transform.position = firstCommitSlot.transform.position;
                    firstJailSlot.AddCardToSlot(firstCard);
                }
                {
                    CardSlot secondJailSlot = secondJailSlots[index];
                    Card secondCard = secondJailSlot.RemoveCardFromSlot();
                    CardSlot secondCommitSlot = _handsToCommitSlots[_slotsToHands[secondJailSlot]][0];

                    secondJailSlot.transform.position = secondCommitSlot.transform.position;
                    secondJailSlot.AddCardToSlot(secondCard);
                }

            }
            yield return new WaitForSeconds(0.5f);
            for (int index = 0; index < secondJailSlots.Count; index++)
            {
                CardSlot firstJailSlot = firstJailSlots[index];
                CardSlot secondJailSlot = secondJailSlots[index];

                firstJailSlot.StartNextTurnArrowEffect();
                yield return StartCoroutine(ApplyCardToStack(firstJailSlot, stack));
                firstJailSlot.StopNextTurnArrowEffect();

                secondJailSlot.StartNextTurnArrowEffect();
                yield return StartCoroutine(ApplyCardToStack(secondJailSlot, stack));
                secondJailSlot.StopNextTurnArrowEffect();
            }

            //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
            //so, big assumptions here.
          
          

        //    yield return StartCoroutine(_scoreHand.AddRound(stack, firstCardSlot, secondCardSlot));
            sides.Add(stack.GetTopTokenSide());
            yield return new WaitForSeconds(0.5f);

        }

        yield return new WaitForSeconds(1f);

        //are all sides the same?
        TokenSide firstSide = sides.PeekFront();
        if (sides.Count(x => x == firstSide) == sides.Count)
        {
            yield return StartCoroutine(DeclareWinner(firstSide));
        }
        else
        {
            //now what?
            Debug.LogError("Don't know how to resolve tie for multi stack games");
            yield return StartCoroutine(TieBreak());
        }

    }

    private IEnumerator DeclareWinner(TokenSide winner)
    {
        Debug.Log("Winner Is: " + winner.ToString());

        yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine(winner.ToString()));
        yield return StartCoroutine(SoundPlayer.Instance.PlaySoundCoroutine("Wins"));

        Application.LoadLevel(0);
    }

    



    public void Init(MatchSettingsSO matchSettings, AIPlayer whitePlayer, AIPlayer blackPlayer)
    {
        _matchSettings = matchSettings;

        //spawn stack
        _stacks = new List<Stack>();
        for (int i = 0; i < _matchSettings.NumberOfStacks; i++)
        {
            Stack stack = StackHandle.InstantiateChild(StackPrefab);
            stack.Init(_matchSettings.StackStyle);
            _stacks.Add(stack);
        }
        _stacks.PositionAlongLineCentered(Vector3.forward, 0.5f, Vector3.up * 0.5f);

        //spawn hands
        _hands = new List<Hand>();
        _cards = new List<Card>();

        _handsToJailCards = new Dictionary<Hand, List<CardSlot>>();
        _handsToCommitSlots = new Dictionary<Hand, List<CardSlot>>();
        _slotsToHands = new Dictionary<CardSlot, Hand>();
        _stacksToCommitSlots = new Dictionary<Stack, List<CardSlot>>();
        _allCommitCardSlots = new List<CardSlot>();
        _allJailCardSlots = new List<CardSlot>();



        foreach (PlayerSO playerSo in _matchSettings.Players)
        {
            Hand hand = transform.InstantiateChild(HandPrefab);
            hand.Init(playerSo, _matchSettings);

            _hands.Add(hand);

            foreach (CardSlot handSlot in hand.Slots)
            {
                _slotsToHands.Add(handSlot, hand);
            }

            //spawn cards
            List<Card> cardsForThisHand = new List<Card>();
            foreach (CardSO cardSo in _matchSettings.CardsPerHand)
            {
                Card card = transform.InstantiateChild(CardPrefab);
                card.Init(cardSo, _matchSettings.StackStyle, playerSo);
                cardsForThisHand.Add(card);
            }

            //spawn playSlots (per hand per stack)

            List<CardSlot> playSlotsForPlayer = new List<CardSlot>();
            foreach (Stack stack in _stacks)
            {
                
                CardSlot commitSlot = hand.transform.InstantiateChild(CardSlotPrefab);
                commitSlot.name = "Commit slot for " + hand.gameObject.name;
                playSlotsForPlayer.Add(commitSlot);
                _slotsToHands.Add(commitSlot, hand);
                _allCommitCardSlots.Add(commitSlot);
                if(!_stacksToCommitSlots.ContainsKey(stack))
                {
                    _stacksToCommitSlots.Add(stack, new List<CardSlot>());
                }
               _stacksToCommitSlots[stack].Add(commitSlot);

                //need this to only happen when a card starts to be dragged
               /* Bounds boundsExpander = commitSlot.collider.bounds;
                boundsExpander.Expand(new Vector3(20f,0f,5));
                commitSlot.GetComponent<BoxCollider>().size = boundsExpander.size;*/
                

            }

           

            _handsToCommitSlots.Add(hand, playSlotsForPlayer);

            for (int index = 0; index < cardsForThisHand.Count; index++)
            {
                CardSlot slot = hand.Slots[index];
                Card card = cardsForThisHand[index];

                card.transform.position = slot.transform.position + Vector3.up*10f;
                hand.AddCardToHand(card);
            }

            _cards.AddRange(cardsForThisHand);

            if(_matchSettings.TieBreaker == TieBreakerStyle.UseJailCards)
            {
                CardSlot jailSlot = hand.transform.InstantiateChild(CardSlotPrefab);
                jailSlot.name = "Jail slot for " + hand.gameObject.name;

                if(!_handsToJailCards.ContainsKey(hand))
                {

                    _handsToJailCards.Add(hand, new List<CardSlot>());
                }
                _handsToJailCards[hand].Add(jailSlot);
                _slotsToHands.Add(jailSlot, hand);
                
                _allJailCardSlots.Add(jailSlot);
            }
        }


        //POSITIONING OF ROOT ELEMENTS
        //reposition hands around a circle
        for (int index = 0; index < _hands.Count; index++)
        {
            float frac = index/(float)_hands.Count;
           
            Hand hand = _hands[index];
            Bounds bounds = hand.transform.RenderBounds();

            hand.transform.position = hand.transform.position.SinCosY(frac) * bounds.size.x;
            
            
            hand.transform.LookAt(Vector3.zero, Vector3.up);

            hand.transform.position = hand.transform.position + Vector3.down * 5f;
            List<CardSlot> jailSlots = _handsToJailCards[hand];
            jailSlots.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.up * 0.5f);

            //todo: reposition after the cards have been chosen.
            foreach (CardSlot jailCardSlot in jailSlots)
            {
                jailCardSlot.transform.localPosition = transform.forward * 15f;

                jailCardSlot.transform.parent = hand.transform.parent;
            }

            List<CardSlot> commitSlots = _handsToCommitSlots[hand];
            commitSlots.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.up * 0.5f);

            foreach (CardSlot commitSlot in commitSlots)
            {


                commitSlot.transform.localPosition = transform.forward * 15f;
                
                commitSlot.transform.parent = hand.transform.parent;
            }
        }



        //spawn score hand
        _scoreHand = transform.InstantiateChild(ScoreHandPrefab);
        _scoreHand.Init(_hands, _matchSettings);

        _scoreHand.transform.localPosition = Vector3.right * ((_scoreHand.transform.RenderBounds().size.x *0.5f) + 10f);
        //spawn cards but don't put them anywhere or maybe put them on the score hand

        //reposition slots infront of stacks

        foreach (Stack stack in _stacks)
        {
        
            MainStack = stack;
        }
       

        if(whitePlayer!=null)
        {
            _whitePlayer = whitePlayer;
            _whitePlayer.Init(_hands[0]);
        }

        if (blackPlayer != null)
        {
            _blackPlayer = whitePlayer;
            _blackPlayer.Init(_hands[1]);
        }

    }


    public void DroppedCardOnCardSlot(Card displacingCard, CardSlot targetSlot, CardSlot previousSlot)
    {
        Hand hand = _slotsToHands[targetSlot];

        if (displacingCard.PlayerSoRef == hand.PlayerSoRef)
        {
             previousSlot.RemoveCardFromSlot();
            
            if (!targetSlot.IsEmpty)
            {
                Card swap = targetSlot.RemoveCardFromSlot();
                previousSlot.AddCardToSlot(swap);
            }
            targetSlot.AddCardToSlot(displacingCard);
            targetSlot.PlaySpecialPlacementEffect();
            SoundPlayer.Instance.PlaySound("PlaceCard");

        }
        //else, let it snap back. clean itself up
    }
    public void ClickedOnCard(Card card)
    {
        CardSlot clickedSlot = card.transform.parent.GetComponent<CardSlot>();
        if (clickedSlot != null)
        {

            Hand hand = _slotsToHands[clickedSlot];
            if (hand != null)//this slot is part of a hand.
            {
                if (hand.Slots.Find(x=> x.CardInSlot == card))
                {
                    if (hand.AutoPlacementTargetSlot.IsInteractive)
                    {
                        if (!hand.AutoPlacementTargetSlot.IsEmpty)
                        {

                            Card swap = hand.AutoPlacementTargetSlot.RemoveCardFromSlot();
                            clickedSlot.RemoveCardFromSlot();

                            hand.AutoPlacementTargetSlot.AddCardToSlot(card);
                            clickedSlot.AddCardToSlot(swap);
                            hand.AutoPlacementTargetSlot.PlaySpecialPlacementEffect();
                        }
                        else
                        {
                            hand.AutoPlacementTargetSlot.AddCardToSlot(clickedSlot.RemoveCardFromSlot());
                            hand.AutoPlacementTargetSlot.PlaySpecialPlacementEffect();
                        }
                        SoundPlayer.Instance.PlaySound("SwapCards");
                    }
                    else
                    {
                        Debug.Log("Target Not interactive: " + hand.AutoPlacementTargetSlot.gameObject.name);
                    }
                }
                else if (_handsToCommitSlots[hand].Contains(clickedSlot))//could be a commit slot or a 
                {
                    Card swap = clickedSlot.RemoveCardFromSlot();
                    _slotsToHands[clickedSlot].AddCardToHand(swap);
                    SoundPlayer.Instance.PlaySound("SwapCards");

                }
                else if (_handsToJailCards[hand].Contains(clickedSlot))
                {
                    Card swap = clickedSlot.RemoveCardFromSlot();
                    _slotsToHands[clickedSlot].AddCardToHand(swap);
                    SoundPlayer.Instance.PlaySound("SwapCards");
                }

                
            }
           
            
                
            else
            {
                Debug.Log("Clicked on a non hand owned slot. Probably score slots. " + clickedSlot.gameObject.name);
            }


       /*     if (_allCommitCardSlots.Contains(clickedSlot))
            {
                //logically, hands will always have an empty slot if you are clicking on a card from a 
                //commit slot
                Card swap = clickedSlot.RemoveCardFromSlot();
                _slotsToHands[clickedSlot].AddCardToHand(swap);
            }*/
        }
    }

}
