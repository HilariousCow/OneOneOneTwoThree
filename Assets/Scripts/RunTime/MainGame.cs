﻿using System;
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
    public Transform ScoreHandle;

    public Transform StackHandle;
  

    public Token DropTokens;

    public Collider DropTablePrefab;
    private Collider _dropTable;

    internal Stack MainStack;
    //public TextMesh TestMeshPrefab;

    //need cards sos
    private MatchSettingsSO _matchSettings;
    private List<Hand> _hands;
    private List<Card> _cards;//all cards
    private ScoreHand _scoreHand;

    private Dictionary<Hand, List<CardSlot>> _handsToJailCards;//not always used
    private Dictionary<Hand,List<CardSlot>> _handsToCommitSlots;
    private Dictionary<CardSlot, Hand> _slotsToHands;

    private List<CardSlot> _allCommitCardSlots;
    private List<CardSlot> _allJailCardSlots;



    private AIPlayer _whitePlayer;
    private AIPlayer _blackPlayer;


    private float _commitTime;

    public List<Hand> Hands
    {
        get { return _hands; }
    }

    void Awake()
    {

        
    }

   

    void OnDestroy()
    {
        CamerFOVController fov = FindObjectOfType<CamerFOVController>();
        if (fov != null)
        {
            fov.SetMainGame(null);
        }
    }

    void Start()
    {
        //temp
        
        StartCoroutine(Toss());
        
    }
    IEnumerator Toss()
    {

        TurnOffScoreHands();
        TurnOffJailSlotInteractivity();
        TurnOffCommitSlotInteractivity();
        TurnOffHands();


        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("OneOneOneTwoThree"));
        yield return new WaitForSeconds(0.5f);
        HelpText.Instance.PlayMessage("Drop the Stack");

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

      
        yield return new WaitForSeconds(0.5f);
        MainStack.CopyTokenPositions(1, other);

        Destroy(other.gameObject);
        yield return new WaitForSeconds(0.5f);


        MainStack.CopyTokenPositions(0, lastToStopMoving);
        Destroy(lastToStopMoving.gameObject);
        Destroy(_dropTable.gameObject);
        yield return new WaitForSeconds(1.5f);
     


        StartCoroutine(IntroPhase());
    }

    
    IEnumerator IntroPhase()
    {
        //randomize stack
        yield return null;
        //see if we need the tie breaker intro
        switch (_matchSettings.TieBreaker)
        {

            case TieBreakerStyle.FlipStack:
                
                break;
            case TieBreakerStyle.UseJailCards:
              
                TurnOnJailSlotInteractivity();
                SetAllHandsToJailCard();

                yield return new WaitForSeconds(.1f);
                TurnOnHands();
                HelpText.Instance.PlayMessage("PickReserve");
                bool all = (_allJailCardSlots.FindAll(x => !x.IsEmpty).Count == _allJailCardSlots.Count);
                bool pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;

                ProdAIForMove();
                _commitTime = 1f;
                while (!all || !pointDownMosty || _commitTime >= 0.0f)
                {

                    yield return null;
                    pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
                    all = (_allJailCardSlots.FindAll(x => !x.IsEmpty).Count == _allJailCardSlots.Count);

                    if(all && pointDownMosty)
                    {
                        _commitTime -= Time.deltaTime;
                    }
                    else
                    {
                        _commitTime = 1f;
                    }

                    MainStack.SetCommitTime(_commitTime);
                }
                TurnOffJailSlotInteractivity();
                yield return new WaitForSeconds(.1f);
                yield return StartCoroutine(MoveJailCardsUnderStack());
                
                break;

            case TieBreakerStyle.GoldenGoal:
                //TODO
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        StartCoroutine(PlayRoundPhase());
       
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
        state.WhitePlayer = Hands[0];
        state.BlackPlayer = Hands[1];
        state.StackState = MainStack;

        return state;//todo: previous plays in order by both.
    }

    private IEnumerator MoveJailCardsUnderStack()
    {
        TokenSide topAtBeginningOfOperation = MainStack.GetTopTokenSide();

        foreach (Hand hand in Hands)
        {
            
            CardSlot jailslot = _handsToJailCards[hand][0];
            Card tieBreakerCard = jailslot.RemoveCardFromSlot();
        
            jailslot.transform.position = MainStack.transform.position + transform.right * 9f;
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
        foreach (Hand hand in Hands)
        {
            CardSlot jailSlot = _handsToJailCards[hand][0];
            hand.AutoPlacementTargetSlot = jailSlot;
            

        }
    }

    private void SetAllHandsToCommitSlot()
    {
        foreach (Hand hand in Hands)
        {
            CardSlot targetSlot = _handsToCommitSlots[hand][0];
            hand.AutoPlacementTargetSlot = targetSlot;
        }

    }

    IEnumerator PlayRoundPhase()
    {
        yield return StartCoroutine(ChooseAndApplyPhase());
        yield return StartCoroutine(ShowScoresPhase());

        if (_scoreHand.FinishedAllRounds)
        {
            Debug.Log("Rounds are over");
            StartCoroutine(ResolutionPhase());
        }
        else
        {
            Debug.Log("Starting new round");
            StartCoroutine(PlayRoundPhase());//go again.    
        }

    }
    IEnumerator ChooseAndApplyPhase()
    {
      
        yield return StartCoroutine(_scoreHand.AnnouceRoundNumber());

        _scoreHand.StartNewRound(); //position current card slots under

        TurnOnHands();
        ClearEmptySlotsInHands();

        TurnOnCommitSlotInteractivity(MainStack.GetTopTokenSide());
        SetAllHandsToCommitSlot();
        TokenSide topAtBeginningOfOperation = MainStack.GetTopTokenSide();

        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(topAtBeginningOfOperation.ToString()));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("GoFirst"));


        //find whose is applied first for this stack


        //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
        //so, big assumptions here.
        CardSlot firstCardSlot =
            _allCommitCardSlots.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide == topAtBeginningOfOperation);


        CardSlot secondCardSlot =
            _allCommitCardSlots.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide != topAtBeginningOfOperation);


        //show first slot to apply graphic.

        Debug.LogWarning("Watching for all slots to be filled");
        bool all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);
        bool pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
        ProdAIForMove();
        _commitTime = 1f;
        while (!all || !pointDownMosty || _commitTime >= 0.0f)
        {

            yield return null;
            pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
            all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);

            if (all && pointDownMosty)
            {
                _commitTime -= Time.deltaTime;
            }
            else
            {
                _commitTime = 1f;
            }
            MainStack.SetCommitTime(_commitTime);
        }

        TurnOffCommitSlotInteractivity();
        TurnOffHands();
        Debug.LogWarning("All slots filled, resolving gameplay");
        //todo: lockout changes. focus camera, or have it above in the first place.
        yield return new WaitForSeconds(1.5f);

        //todo: if both stacks are not the same on top, send back the cards.


        //find whose is applied first for this stack


        //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.


        //first to go
     
        yield return StartCoroutine(ApplyCardToStack(firstCardSlot, MainStack));

     
        yield return StartCoroutine(ApplyCardToStack(secondCardSlot, MainStack));

        yield return new WaitForSeconds(0.5f);


        


   //     firstCardSlot.CardInSlot.transform.parent = firstCardSlot.transform;
  //      secondCardSlot.CardInSlot.transform.parent = firstCardSlot.transform;

        firstCardSlot.CardInSlot.ShowDescriptionText(false);
        secondCardSlot.CardInSlot.ShowDescriptionText(false);


        MainStack.transform.parent = transform;
        StackHandle.transform.localPosition = Vector3.zero;
        MainStack.transform.parent = StackHandle;


    }


    IEnumerator ShowScoresPhase()
    {
        CardSlot firstCardSlot =
           _allCommitCardSlots.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide == MainStack.GetTopTokenSide());
        CardSlot secondCardSlot =
            _allCommitCardSlots.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide != MainStack.GetTopTokenSide());

    //moving to score slots
        //no longer occurring?
    
        TurnOnScoreHands();


        _scoreHand.transform.parent = transform;
        ScoreHandle.transform.localPosition = -_scoreHand.GetCurrentRoundSlotPosition();
        ScoreHandle.transform.localRotation = Quaternion.identity;

        _scoreHand.transform.parent = ScoreHandle;
        yield return new WaitForSeconds(0.5f);//give it some time to position or the positions will be screwey

        yield return StartCoroutine(_scoreHand.RoundResolution(MainStack, firstCardSlot, secondCardSlot));
       
        yield return new WaitForSeconds(0.5f);

        _scoreHand.transform.parent = transform;
        ScoreHandle.transform.localPosition = Vector3.zero;
        ScoreHandle.transform.localRotation = Quaternion.AngleAxis(90, Vector3.up);
        _scoreHand.transform.parent = ScoreHandle;

        yield return StartCoroutine(_scoreHand.TellMeTheScores());
        
     
        _scoreHand.transform.parent = transform;
        ScoreHandle.transform.localPosition = Vector3.right * 20f;
        ScoreHandle.transform.localRotation = Quaternion.AngleAxis(90, Vector3.forward) * Quaternion.AngleAxis(90, Vector3.up);
        _scoreHand.transform.parent = ScoreHandle;


    }

    private void ClearEmptySlotsInHands()
    {
        foreach (Hand hand in Hands)
        {
            hand.ClearOutEmptySlotsAndReorganize();
        }
    }

    

    private void TurnOffHands()
    {
        foreach (Hand hand in Hands)
        {
            hand.ShowHand(false);
            //hand.gameObject.SetActive(false);
        }
    }


    private void TurnOnHands()
    {
        foreach (Hand hand in Hands)
        {
            hand.ShowHand(true);
         //   hand.gameObject.SetActive(true);
        }
    }

    IEnumerator ApplyCardToStack( CardSlot slot, Stack stack)
    {
        Vector3 slotStartPos = slot.transform.localPosition;
        Vector3 slotShowOffPosition = slot.transform.position/1.5f + Vector3.up*15f;
        Card firstCard = slot.CardInSlot;
    //    Debug.Log("Showing first card" + firstCard.gameObject.name + " from slot: " + slot.gameObject.name);
        firstCard.IdleAnim();
        firstCard.transform.parent = transform;

        slot.transform.position = slotShowOffPosition;

        slot.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.right);
        //firstCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.right);
        firstCard.transform.parent = slot.transform;
        firstCard.ShowDescriptionText(true);
        yield return StartCoroutine(firstCard.PreviewStack.AnimateCardEffectOnStack(firstCard));
        
        firstCard.IdleAnim();
        yield return new WaitForSeconds(0.3f);//show top for 0.5
        //move stack to new target pos

        stack.transform.parent = transform;
        stack.transform.rotation = Quaternion.LookRotation(-firstCard.transform.position.FlatY(), Vector3.up);

        StackHandle.transform.position = firstCard.PreviewStack.transform.position + Vector3.up * 5f;
        StackHandle.transform.rotation = firstCard.PreviewStack.transform.rotation;
   
        stack.transform.parent = StackHandle;
        yield return new WaitForSeconds(1.0f);//allow it to get there

        StartCoroutine(firstCard.PreviewStack.AnimateCardEffectOnStack(firstCard));
        yield return StartCoroutine(stack.AnimateCardEffectOnStack(firstCard));
      
        yield return new WaitForSeconds(1.0f);//show top for 0.5

        stack.transform.parent = transform;

        StackHandle.transform.position = StackHandle.transform.position.FlatZ().FlatX();
        StackHandle.transform.rotation = Quaternion.LookRotation(-firstCard.transform.position.FlatY(), Vector3.up);

        stack.transform.parent = StackHandle;

        //todo: unflip after both are applied.
        firstCard.transform.parent = transform;
        slot.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.right);
        slot.transform.localPosition = slotStartPos;

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

    private void TurnOnCommitSlotInteractivity(TokenSide startingCard)
    {
        Debug.Log("Turn on commit slots");
        foreach (CardSlot slot in _allCommitCardSlots)
        {
            Hand owner = _slotsToHands[slot];
            float delay = owner.PlayerSoRef.DesiredTokenSide == startingCard ? 0.0f : 1f;
            slot.SetHighlightDelay(delay);
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
        if( _scoreHand.GameIsATie )//temp pretend it's always a tie.
        {
            Debug.Log("Game is a draw");
            yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("TieGame"));
            yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("TieBreaker"));

            switch (_matchSettings.TieBreaker)
            {
                case TieBreakerStyle.FlipStack:
                    yield return StartCoroutine(TieBreakFilp());
                    break;
                case TieBreakerStyle.UseJailCards:
                    yield return StartCoroutine(TieBreakJailCards());
                    break;
                case TieBreakerStyle.GoldenGoal:
                    yield return StartCoroutine(TieBreakGoldenGoal());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
       
        Card tieBreaker = transform.InstantiateChild(CardPrefab);
        tieBreaker.Init(_matchSettings.TieBreakerCard, _matchSettings.StackStyle,
                        _matchSettings.TieBreakerPlayer);
        yield return new WaitForSeconds(0.5f);
        tieBreaker.transform.position = Vector3.up * 2f;
        yield return new WaitForSeconds(0.5f);
        tieBreaker.transform.rotation = tieBreaker.transform.rotation *
                                        Quaternion.AngleAxis(180f, Vector3.forward);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(MainStack.AnimateCardEffectOnStack(tieBreaker));
       
        yield return new WaitForSeconds(0.5f);

        Destroy(tieBreaker.gameObject);
        //find whose is applied first for this stack
       

        yield return new WaitForSeconds(1f);


        yield return StartCoroutine(DeclareWinner(MainStack.GetTopTokenSide()));
      
    }

    private IEnumerator TieBreakGoldenGoal()
    {
        yield return StartCoroutine(ChooseAndApplyPhase());//one more phase but don't score
        //are all sides the same?
        TokenSide firstSide = MainStack.GetTopTokenSide();

        yield return StartCoroutine(DeclareWinner(firstSide));
    }

    private IEnumerator TieBreakJailCards()
    {
        Debug.Log("Doing tie breaker cards.");
        yield return null;

        TokenSide topAtBeginningOfOperation = MainStack.GetTopTokenSide();
      
        //find whose is applied first for this stack
        List<CardSlot> firstJailSlots =
            _handsToJailCards[Hands.Find(x => x.PlayerSoRef.DesiredTokenSide == topAtBeginningOfOperation)];

        List<CardSlot> secondJailSlots =
            _handsToJailCards[Hands.Find(x => x.PlayerSoRef.DesiredTokenSide != topAtBeginningOfOperation)];

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


            yield return StartCoroutine(ApplyCardToStack(firstJailSlot, MainStack));



            yield return StartCoroutine(ApplyCardToStack(secondJailSlot, MainStack));


            yield return new WaitForSeconds(0.5f);


            firstJailSlot.CardInSlot.transform.parent = firstJailSlot.transform;
            secondJailSlot.CardInSlot.transform.parent = secondJailSlot.transform;

            firstJailSlot.CardInSlot.ShowDescriptionText(false);
            secondJailSlot.CardInSlot.ShowDescriptionText(false);

            MainStack.transform.parent = null;

            StackHandle.transform.position = Vector3.zero;

            MainStack.transform.parent = StackHandle;
        }

        //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
        //so, big assumptions here.
      
      

//    yield return StartCoroutine(_scoreHand.RoundResolution(stack, slot, secondCardSlot));
      yield return new WaitForSeconds(1.5f);

        


        //are all sides the same?
        TokenSide firstSide = MainStack.GetTopTokenSide();
      
        yield return StartCoroutine(DeclareWinner(firstSide));
       

    }

    private IEnumerator DeclareWinner(TokenSide winner)
    {
        Debug.Log("Winner Is: " + winner.ToString());

        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine(winner.ToString()));
        yield return StartCoroutine(HelpText.Instance.PlayMessageCoroutine("Wins"));

     
        GameSpawner.Instance.TurnOn();
        Destroy(gameObject);
    }

    



    public void Init(MatchSettingsSO matchSettings, AIPlayer whitePlayer, AIPlayer blackPlayer)
    {
        if (whitePlayer != null)
        {
            _whitePlayer = transform.InstantiateChild<AIPlayer>(AIPrefab);
        }

        if (blackPlayer != null)
        {
            _blackPlayer = transform.InstantiateChild<AIPlayer>(AIPrefab);
        }

        _matchSettings = matchSettings;

        //spawn stack

        MainStack = StackHandle.InstantiateChild(StackPrefab);
        MainStack.Init(_matchSettings.StackStyle);
           
        
       
        //spawn hands
        _hands = new List<Hand>();
        _cards = new List<Card>();

        _handsToJailCards = new Dictionary<Hand, List<CardSlot>>();
        _handsToCommitSlots = new Dictionary<Hand, List<CardSlot>>();
        _slotsToHands = new Dictionary<CardSlot, Hand>();
        
        _allCommitCardSlots = new List<CardSlot>();
        _allJailCardSlots = new List<CardSlot>();



        foreach (PlayerSO playerSo in _matchSettings.Players)
        {
            Hand hand = transform.InstantiateChild(HandPrefab);
            hand.Init(playerSo, _matchSettings);

            Hands.Add(hand);

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
            
                
            CardSlot commitSlot = hand.transform.InstantiateChild(CardSlotPrefab);
            commitSlot.name = "Commit slot for " + hand.gameObject.name;
            playSlotsForPlayer.Add(commitSlot);
            _slotsToHands.Add(commitSlot, hand);
            _allCommitCardSlots.Add(commitSlot);
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
        for (int index = 0; index < Hands.Count; index++)
        {
            float frac = index/(float)Hands.Count;
           
            Hand hand = Hands[index];
            Bounds bounds = hand.transform.RenderBounds();

            hand.transform.position = hand.transform.position.SinCosY(frac) * bounds.size.x*0.5f;
            
            
            hand.transform.LookAt(Vector3.zero, Vector3.up);

            hand.transform.position = hand.transform.position + Vector3.down * 5f;
            
            if (_matchSettings.TieBreaker == TieBreakerStyle.UseJailCards)
            {
                List<CardSlot> jailSlots = _handsToJailCards[hand];
                jailSlots.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.up*0.5f);

                //todo: reposition after the cards have been chosen.
                foreach (CardSlot jailCardSlot in jailSlots)
                {
                    jailCardSlot.transform.position = hand.transform.position/2f;

                    jailCardSlot.transform.parent = hand.transform.parent;
                }
            }
            List<CardSlot> commitSlots = _handsToCommitSlots[hand];
            commitSlots.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.up * 0.5f);

            foreach (CardSlot commitSlot in commitSlots)
            {


                commitSlot.transform.position = hand.transform.position / 2f;
                
                commitSlot.transform.parent = hand.transform.parent;
            }
        }


        //spawn score hand
        _scoreHand = ScoreHandle.InstantiateChild(ScoreHandPrefab);
        _scoreHand.Init(Hands, _matchSettings);

        ScoreHandle.transform.localPosition = Vector3.right * ((_scoreHand.transform.RenderBounds().size.x * 0.75f) + 10f);
        //spawn cards but don't put them anywhere or maybe put them on the score hand

        //reposition slots infront of stacks

        if (_whitePlayer != null)
        {
         
            _whitePlayer.Init(Hands[0]);
        }

        if (_blackPlayer != null)
        {
           
            _blackPlayer.Init(Hands[1]);
        }

        GetComponent<HelpText>().Init(this);
        _dropTable = transform.InstantiateChild(DropTablePrefab.gameObject).GetComponent<Collider>();
        FindObjectOfType<CamerFOVController>().SetMainGame(this);
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


    void Update()
    {
        //temp fastforward
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 4f;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Time.timeScale = 1f;
        }
    }
}
