using UnityEngine;
using System.Collections;

public class SlotHighlightEffect : MonoBehaviour
{
    private Renderer _rend;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
    }
	
	

    internal void StopEffect()
    {
        _rend.enabled = false;
    }

    internal void StartEffect()
    {
        _rend.enabled = true;
    }
}
