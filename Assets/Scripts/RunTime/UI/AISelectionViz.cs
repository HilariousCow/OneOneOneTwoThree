using UnityEngine;
using System.Collections;

public class AISelectionViz : MonoBehaviour
{
    public AIPlayer AiPlayerPrefab { get; private set; }

	

    internal void Init(AIPlayer aiPlayer)
    {
        AiPlayerPrefab = aiPlayer;
    }
}
