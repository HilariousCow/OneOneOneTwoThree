using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//This is a pre-shuffled deck so that we can peek at the next card
//Need a source of cards and a rng
//Can be set up to re-shuffle so that you can draw from it infinitely.
//Note: assumes that the "Original Deck" is never changed, although if you wanted to you could add or remove stuff from the source list and
//  the changes would be picked up in the next shuffle
[Serializable]
public class CardDeck<T> where T : ScriptableObject // this could work with any class but we enforce this use case for now
{

	//potentially: select the next card so you can "peek"?
    //won't work for predicates though
    [HideInInspector]
    private List<T> _drawPile;
    [HideInInspector]
    private MonoRandom _rand;
    private bool _reshuffleWhenEmpty;
    private List<T> _discardPile;
    

  //  public bool CanDraw { get { return _reshuffleWhenEmpty || _shuffledCopy.Count > 0; } }
    
    public CardDeck(List<T> originalDeck, MonoRandom rand, bool reshuffleWhenEmpty)
    {
		_discardPile = new List<T>(originalDeck);
        _drawPile = new List<T>();

        _reshuffleWhenEmpty = reshuffleWhenEmpty;
        _rand = rand;
        Shuffle();
    }

    private void Shuffle()
	{
        _drawPile.AddRange(_rand.ShuffleList(_discardPile));
    }


	public T DrawCard()
	{
		return DrawCard(x => true);
	}

	public T DrawCard(Predicate<T> match)
	{
		T result = DrawAndReadCard(match);

		if (result != null)
		{
			return result;
		}

		// if we can't reshuffle then we should stop now
		if (_reshuffleWhenEmpty)
		{
            Shuffle();
			return DrawAndReadCard(match);
		}
		else
		{
			return null;
		}
		
	}

	private T DrawAndReadCard(Predicate<T> match)
	{
		for (int i = 0; i < _drawPile.Count; ++i)
		{
			T card = _drawPile[i];
			
			if (match(card))
			{
				_drawPile.RemoveAt(i);
				_discardPile.Add(card);
				return card;
			}
		}

		return null;
	}


}

