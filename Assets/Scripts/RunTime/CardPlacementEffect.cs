using UnityEngine;
using System.Collections;

public class CardPlacementEffect : MonoBehaviour
{
    private Material _mat;
    public float InnerEdge;//animates shader
    public float OuterEdge;//animates shader
    private Renderer _rend;
    private Animator _anim;
    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rend = renderer;
        _mat = _rend.material;
    }



    public void Play()
    {
        _anim.SetTrigger("PlayEffect");
    }

    void LateUpdate()
    {
        _mat.SetFloat("_InnerEdge", InnerEdge);
        _mat.SetFloat("_OuterEdge", OuterEdge);
    }
}
