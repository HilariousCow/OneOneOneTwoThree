using UnityEngine;
using System.Collections;

public class NextTurnArrow : MonoBehaviour
{

    private Renderer _rend;

    void Awake()
    {
        _rend = renderer;
    }
    internal void StartEffect()
    {
        _rend.enabled = true;
    }

    internal void StopEffect()
    {
        _rend.enabled = false;
    }
}
