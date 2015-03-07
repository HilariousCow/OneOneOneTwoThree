using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    public Stack StackPrefab;
    private CardSO _cardSoRef;

    private Stack _previewStack;
    private Renderer _rend;

	public void Init(CardSO cardSo, StackSO stackSo, PlayerSO playerSo)
	{
	    _cardSoRef = cardSo;
	    _rend = renderer;

	    _rend.material = playerSo.CardMaterial;


	    //_previewStack = //hmm. how to make this work as preview and as main stack
	}
}
