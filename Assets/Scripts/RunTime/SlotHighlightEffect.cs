using UnityEngine;
using System.Collections;

public class SlotHighlightEffect : MonoBehaviour
{
    private int hashPlayEffect = Animator.StringToHash("PlayEffect");
    private Animator _anim;
    private Renderer _rend;
    private float _highlightDelay = 0.0f;
    void Awake()
    {
        _rend = renderer;
        _anim = GetComponent<Animator>();
    }
	
	

    internal void StopEffect()
    {
        _rend.enabled = false;
    }

    internal void StartEffect()
    {

        //next time is
        float nextStartTime = Mathf.FloorToInt(Time.time/3f) * 3f + 3f;
        float delay = nextStartTime - Time.time + _highlightDelay;

        Debug.Log("delay" + delay);
        Invoke("Play", delay);
      
      
    }
    void Play()
    {
        _rend.enabled = true;

        _anim.SetTrigger(hashPlayEffect);

    }
    internal void SetDelay(float delay)
    {
        Debug.Log("setting delay");
        _highlightDelay = delay;
    }
}
