using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AIPlayer : MonoBehaviour
{
    public delegate void AIEvent(CardSlot cardSlot, AIPlayer ai);
    public AIEvent CardChosen = delegate { };

    private Hand _myHand;
    //hand that you own
    public void Init(Hand hand)
    {
        _myHand = hand;
    }

    public void ChooseCardForGameState(GameState state)
    {
        StartCoroutine(ChooseCardAndSignalWhenDone(state));
    }

    private IEnumerator ChooseCardAndSignalWhenDone(GameState state)
    {
        yield return null;

        List<CardSlot> cardsLeft = _myHand.Slots.FindAll(x => !x.IsEmpty);

        //simplest = choose random card

        yield return new WaitForSeconds(1f);//at least pretend to wait
        CardSlot chosenSlot = null;
        while (chosenSlot==null)
        {
            chosenSlot = cardsLeft[Random.Range(0, cardsLeft.Count)];
            yield return null;//use pseudo threading if you need to do computationally expensive things.
        }

        CardChosen.Invoke(chosenSlot, this);//invoke this, which the main thread is waiting for. no take backs (for now)
    }

}
