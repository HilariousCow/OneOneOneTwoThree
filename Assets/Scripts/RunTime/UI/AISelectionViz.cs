using UnityEngine;
using System.Collections;

public class AISelectionViz : MonoBehaviour
{
    public AIPlayer AiPlayerPrefab { get; private set; }
    public TextMesh NameOfAi;
	

    internal void Init(AIPlayer aiPlayer)
    {
        if(aiPlayer != null)
        {
            NameOfAi.text = aiPlayer.gameObject.name;
        }
        else
        {
            NameOfAi.text = "You";
            NameOfAi.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
        }
        AiPlayerPrefab = aiPlayer;
    }
}
