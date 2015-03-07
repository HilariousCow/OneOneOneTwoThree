using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//override for different game rules.
public class MainGame : MonoBehaviour
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


    void Awake()
    {
        Init(MatchToUseDefault);
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

        foreach (PlayerSO playerSo in _matchSettings.Players)
        {
            Hand hand = transform.InstantiateChild(HandPrefab);
            hand.Init(playerSo, _matchSettings);

            _hands.Add(hand);

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
                CardSlot playSlot = stack.transform.InstantiateChild(CardSlotPrefab);
                playSlotsForPlayer.Add(playSlot);
                _slotsToHands.Add(playSlot, hand);

               

            }

            for (int index = 0; index < playSlotsForPlayer.Count; index++)
            {
                float frac = index / (float)playSlotsForPlayer.Count;
               
                CardSlot slot = playSlotsForPlayer[index];
                Bounds bounds = slot.transform.RenderBounds();

                slot.transform.position = slot.transform.position.SinCosY(frac) * bounds.size.x;
                slot.transform.LookAt(Vector3.zero, Vector3.up);
            }

            _handsToPlaySlots.Add(hand, playSlotsForPlayer);

            foreach (Card card in cardsForThisHand)
            {
                hand.AddCardToHand(card);
            }

            _cards.AddRange(cardsForThisHand);
        }



        //spawn score hand
        _scoreHand = transform.InstantiateChild(ScoreHandPrefab);
        _scoreHand.Init(_hands, _matchSettings);

        
        //POSITIONING OF ROOT ELEMENTS
        for (int index = 0; index < _hands.Count; index++)
        {
            float frac = index/(float)_hands.Count;
           
            Hand hand = _hands[index];
            Bounds bounds = hand.transform.RenderBounds();

            hand.transform.position = hand.transform.position.SinCosY(frac) * bounds.size.x;
            hand.transform.LookAt(Vector3.zero, Vector3.up);
        }

        //spawn cards but don't put them anywhere or maybe put them on the score hand


        //reposition stacks
        //reposition hands around a circle



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
    
}
