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
        Debug.Log("Flipping: " + gameObject.name);
        CurrentSide = (TokenSide)((int) (CurrentSide + 1)%2);
        transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.right);

        SoundPlayer.Instance.PlaySound("PlaceCard");
        if (Effect != null)
        {
            Effect.Play();
        }
    }


    public CardPlacementEffect Effect;
    internal void SetSide(TokenSide topSide)
    {

        CurrentSide = topSide;
        if (CurrentSide == TokenSide.Black)
        {
            transform.localRotation = Quaternion.AngleAxis(180f, Vector3.right);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        if (collision.relativeVelocity.magnitude > 4)
        {
            SoundPlayer.Instance.PlaySound("PlaceCard");
            if(Effect != null)
            {
                Effect.Play();
            }
        }



    }

}
