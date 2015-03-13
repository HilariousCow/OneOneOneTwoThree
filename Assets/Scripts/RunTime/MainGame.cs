using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//override for different game rules.
public class MainGame : MonoBehaviour, IDropCardOnCardSlot, IPointerClickOnCard
{
    public MatchSettingsSO MatchToUseDefault;
    

    public Hand HandPrefab;//hands are effectively "the player"
    public Card CardPrefab;
    public Stack StackPrefab;
    public CardSlot CardSlotPrefab;
    public ScoreHand ScoreHandPrefab;

    //need cards sos
    private MatchSettingsSO _matchSettings;
    private List<Hand> _hands;
    private List<Card> _cards;//all cards
    private List<Stack> _stacks;//all cards
    private ScoreHand _scoreHand;

    private Dictionary<Hand,List<CardSlot>> _handsToPlaySlots;
    private Dictionary<CardSlot, Hand> _slotsToHands;

    private Dictionary<Stack, List<CardSlot>> _stacksToSlots;
    private List<CardSlot> _allCommitCardSlots;

    void Awake()
    {
        Init(MatchToUseDefault);
    }

    void Start()
    {
        StartCoroutine("LoopPhase");
        
    }

    IEnumerator LoopPhase()
    {
        Debug.LogWarning("Watching for all slots to be filled");
        bool all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);
        bool pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
        while (!all || !pointDownMosty)
        {

            yield return null;
            pointDownMosty = Vector3.Dot(Vector3.down, Camera.main.transform.forward) > 0.707f;
            all = (_allCommitCardSlots.FindAll(x => !x.IsEmpty).Count == _allCommitCardSlots.Count);
        }

        Debug.LogWarning("All slots filled, resolving gameplay");
        //todo: lockout changes. focus camera, or have it above in the first place.
        yield return new WaitForSeconds(1f);

        //todo: if both stacks are not the same on top, send back the cards.

        foreach (Stack stack in _stacks)
        {
            //find whose is applied first for this stack
            TokenSide currentTop = stack.GetTopTokenSide();
            List<CardSlot> slotsForStack = new List<CardSlot>(_stacksToSlots[stack] );

            //todo: extract token side orders. yeah. much nicer. but how will black/white determin multiple players? arhgh.
            //so, big assumptions here.

            CardSlot firstCardSlot = slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide == currentTop);
            Card firstCard = firstCardSlot.Card;
            Debug.Log("Showing first card" + firstCard.gameObject.name + " from slot: " + firstCardSlot.gameObject.name);
            
            firstCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, firstCardSlot.transform.forward);
            firstCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.forward);
            
            yield return new WaitForSeconds(1.0f);//show top for 0.5

            stack.ApplyCardToStack(firstCard);
            yield return new WaitForSeconds(0.250f);//show top for 0.5

            firstCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, firstCardSlot.transform.forward);
            firstCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.forward);
            
            yield return new WaitForSeconds(1.0f);//show top for 0.5
            
            
            

            CardSlot secondCardSlot = slotsForStack.Find(x => _slotsToHands[x].PlayerSoRef.DesiredTokenSide != currentTop);
            Card secondCard = secondCardSlot.Card;
            Debug.Log("Showing second card" + secondCard.gameObject.name + " from slot: " + secondCardSlot.gameObject.name);


            secondCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, secondCardSlot.transform.forward);
            secondCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.forward);
            
            
            yield return new WaitForSeconds(1.0f);//show top for 0.5
            stack.ApplyCardToStack(secondCard);
            yield return new WaitForSeconds(0.250f);//show top for 0.5

            secondCardSlot.transform.rotation *= Quaternion.AngleAxis(180f, secondCardSlot.transform.forward);
            secondCard.transform.localRotation *= Quaternion.AngleAxis(160f, Vector3.forward);
            

            yield return new WaitForSeconds(1.5f);//show top for 0.5
            
            


            firstCardSlot.RemoveCardFromSlot();
            secondCardSlot.RemoveCardFromSlot();
            _scoreHand.AddRound(stack, firstCard, secondCard);

            yield return new WaitForSeconds(0.5f);

        }

        if (_scoreHand.FinishedRound)
        {
            Debug.Log("Rounds are over");
            StartCoroutine("ResolutionPhase");
        }
        else
        {
            Debug.Log("Starting new round");
            StartCoroutine("LoopPhase");//go again.    
        }

        

    }

    IEnumerator ResolutionPhase()
    {
        yield return new WaitForSeconds(1f);
        if(_scoreHand.GameIsATie)
        {
            Debug.Log("Game is a draw");
            List<TokenSide> sides = new List<TokenSide>();
            foreach (Stack stack in _stacks)
            {
                Card tieBreaker = transform.InstantiateChild(CardPrefab);
                tieBreaker.Init(_matchSettings.TieBreakerCard, _matchSettings.StackStyle, _matchSettings.TieBreakerPlayer);
                yield return new WaitForSeconds(0.5f);
                tieBreaker.transform.position = Vector3.up*2f;
                yield return new WaitForSeconds(0.5f);
                tieBreaker.transform.rotation = tieBreaker.transform.rotation*
                                                Quaternion.AngleAxis(180f, Vector3.forward);
                yield return new WaitForSeconds(0.5f);
                stack.ApplyCardToStack(tieBreaker);
                yield return new WaitForSeconds(0.5f);
               

                
                
                Destroy(tieBreaker.gameObject);
                //find whose is applied first for this stack
                sides.Add(stack.GetTopTokenSide());
                
            }

            yield return new WaitForSeconds(1f);

            //are all sides the same?
            TokenSide firstSide = sides.PeekFront();
            if(sides.Count(x=>x == firstSide) == sides.Count)
            {
                DeclareWinner(firstSide);
            }
            else
            {
                //now what?
                Debug.LogError("Don't know how to resolve tie for multi stack games");
            }

        }
        else
        {
            
            List<KeyValuePair<Hand, int>> sortedDict = (from entry in _scoreHand.TotalledScores orderby entry.Value descending select entry).ToList();
            DeclareWinner(sortedDict.PeekFront().Key.PlayerSoRef.DesiredTokenSide);

        }
    }

    private void DeclareWinner(TokenSide winner)
    {
        Debug.Log("Winner Is: " + winner.ToString());

        Application.LoadLevel(0);
    }

    

    public void Init(MatchSettingsSO matchSettings)
    {
        _matchSettings = matchSettings;

        //spawn stack
        _stacks = new List<Stack>();
        for (int i = 0; i < _matchSettings.NumberOfStacks; i++)
        {
            Stack stack = transform.InstantiateChild(StackPrefab);
            stack.Init(_matchSettings.StackStyle);
            _stacks.Add(stack);
        }
        _stacks.PositionAlongLineCentered(Vector3.forward, 0.5f, Vector3.up * 0.5f);

        //spawn hands
        _hands = new List<Hand>();
        _cards = new List<Card>();
        _handsToPlaySlots = new Dictionary<Hand, List<CardSlot>>();
        _slotsToHands = new Dictionary<CardSlot, Hand>();
        _stacksToSlots = new Dictionary<Stack, List<CardSlot>>();
        _allCommitCardSlots = new List<CardSlot>();
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
                
                CardSlot commitSlot = stack.transform.InstantiateChild(CardSlotPrefab);
                commitSlot.name = "Commit slot for " + hand.gameObject.name;
                playSlotsForPlayer.Add(commitSlot);
                _slotsToHands.Add(commitSlot, hand);
                _allCommitCardSlots.Add(commitSlot);
                if(!_stacksToSlots.ContainsKey(stack))
                {
                    _stacksToSlots.Add(stack, new List<CardSlot>());
                }
               _stacksToSlots[stack].Add(commitSlot);

            }

           

            _handsToPlaySlots.Add(hand, playSlotsForPlayer);

            foreach (Card card in cardsForThisHand)
            {
                hand.AddCardToHand(card);
            }

            _cards.AddRange(cardsForThisHand);
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
        }

        //spawn score hand
        _scoreHand = transform.InstantiateChild(ScoreHandPrefab);
        _scoreHand.Init(_hands, _matchSettings);

        //spawn cards but don't put them anywhere or maybe put them on the score hand

        //reposition slots infront of stacks

        foreach (Stack stack in _stacks)
        {
            List<CardSlot> slots = _stacksToSlots[stack];
            //slots.PositionAlongLineCentered(Vector3.right, 0.5f, Vector3.up * 0.5f);
            for (int index = 0; index < slots.Count; index++)
            {
                float frac = index / (float)_hands.Count;

                CardSlot slot = slots[index];
                Bounds bounds = slot.transform.RenderBounds();

                slot.transform.position = slot.transform.position.SinCosY(frac) * bounds.size.z;
                slot.transform.LookAt(Vector3.zero, Vector3.up);
            }

        }
       
        
        



        //begin match - call to initiate


    }

    //phases/signals

    //Phase: Initiate Game
    //give cards to hands


    //randomize stack

    //Phase deal hands
    //give cards to players


    //maybe don't do this yet, but, yeah.
    //Wait for both to discard a card to jail
    //Confirm timer...


    //Phase: Loop
    //Wait for both players to play card.
    
    //Confirm timer when both have place and phone is facing down...

    //reveal played cards in order (using stack top to decide)

    //apply cards to stack


    //show winner
    //move cards to played area




    //Phase: Final resolutions
    //quick rematch/back to main menu

    public void DroppedCardOnCardSlot(Card displacingCard, CardSlot targetSlot, CardSlot previousSlot)
    {
       
        Hand hand = _slotsToHands[targetSlot];

        if (displacingCard.PlayerSoRef == hand.PlayerSoRef)
        {
            if (previousSlot != null)
            {
                previousSlot.RemoveCardFromSlot();
            }

            if (!targetSlot.IsEmpty)
            {
                Card swapOut = targetSlot.RemoveCardFromSlot();
                hand.AddCardToHand(swapOut);
            }

            targetSlot.AddCardToSlot(displacingCard);
        }
        //else, let it snap back. clean itself up
    }

    public void ClickedOnCard(Card card)
    {
        CardSlot clickedSlot = card.transform.parent.GetComponent<CardSlot>();
        if(clickedSlot!=null)
        {
            Hand hand = _hands.FirstOrDefault(x => x.Slots.Contains(clickedSlot));
            if (hand != null)
            {
                List<CardSlot> emptyCommitSlots = _handsToPlaySlots[hand].Where(x => x.IsEmpty).ToList();
                if (emptyCommitSlots.Count > 0)
                {
                    foreach (CardSlot commitSlot in emptyCommitSlots)
                    {
                        commitSlot.AddCardToSlot(clickedSlot.RemoveCardFromSlot());
                        break;
                    }
                }
                else
                {
                    Debug.Log("swapping out cards");
                    CardSlot targetSlot = _handsToPlaySlots[hand][0];

                    Card clickedCard = clickedSlot.RemoveCardFromSlot();
                    Card swapOut = targetSlot.RemoveCardFromSlot();

                    targetSlot.AddCardToSlot(clickedCard);
                    clickedSlot.AddCardToSlot(swapOut);

                }
            }

            if (_allCommitCardSlots.Contains(clickedSlot))
            {
                //logically, hands will always have an empty slot if you are clicking on a card from a 
                //commit slot
                Card swap = clickedSlot.RemoveCardFromSlot();
                _slotsToHands[clickedSlot].AddCardToHand(swap);
            }
        }

        

    }

   
}
