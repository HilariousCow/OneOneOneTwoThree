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

    //need cards sos
    private MatchSettingsSO _matchSettings;
    private List<Hand> _hands;
    private List<Card> _cards;//all cards

    public void Init(MatchSettingsSO matchSettings)
    {
        _matchSettings = matchSettings;

        //spawn stack
        //spawn hands
        //spawn cards


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
