using UnityEngine;
using System.Collections;

public class AISelectionViz : MonoBehaviour
{
    private AIPlayer _aiPlayerPrefab;
	

    internal void Init(AIPlayer aiPlayer)
    {
        _aiPlayerPrefab = aiPlayer;
    }
}
