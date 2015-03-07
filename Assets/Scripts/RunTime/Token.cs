using UnityEngine;
using System.Collections;

public enum TokenSide
{
    Black,
    White
}

public class Token : MonoBehaviour
{

    public TokenSide CurrentSide;
    //change the data itself
    //animated by the stack, though.
    //this is called when the animation ends, typically.
    internal void Flip()
    {
        CurrentSide = (TokenSide)((int) (CurrentSide + 1)%2);
        transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.right);
    }

   
}
