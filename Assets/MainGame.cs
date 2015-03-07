using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//override for different game rules.
public class MainGame : MonoBehaviour
{
    public MatchSettingsSO MatchToUseDefault;
    public Hand HandPrefab;
    public Stack StackPrefab;

    //need cards sos
    private MatchSettingsSO _matchSettings;


    public void Init(MatchSettingsSO matchSettings)
    {
        _matchSettings = matchSettings;


    }

    //phases/signals

    //Phase: Initiate Game
    //create hands (which create cards from the ones passed in)
    //randomize stack

    //maybe don't do this yet, but, yeah.
    //Wait for both to discard a card to jail
    //Confirm timer...

    //Phase: Loop
    //Wait for both players to play card.
    
    //Confirm timer...
    //show played cards
    //apply cards to stack
    //show winner
    //move cards to played area




    //Phase: Final resolutions

    
}
