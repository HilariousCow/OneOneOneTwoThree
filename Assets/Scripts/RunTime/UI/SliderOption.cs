using UnityEngine;
using System.Collections;

public class SliderOption<T> : MonoBehaviour where T : Object
{
    public T SelectedItem { get; private set; }
    public TextMesh NameOfAi;
	

    internal void Init(T aiPlayer)
    {
        if(aiPlayer != null)
        {
            NameOfAi.text = aiPlayer.name;
        }
        else
        {
            NameOfAi.text = "You";
            NameOfAi.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.forward);
        }
        SelectedItem = aiPlayer;
    }
}
