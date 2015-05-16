using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelpText : Singleton<HelpText>
{

    private TextMesh WhitePlayerText;
    private TextMesh BlackPlayerText;
    
    private TextMesh[] _texts;
      
	// Use this for initialization
	public void Init (MainGame mainGame)
	{


        Hand[] hands = mainGame.Hands.ToArray();
        List<TextMesh> textInHands = new List<TextMesh>();
	    foreach (Hand hand in hands)
	    {
	        TextMesh foundText = hand.HelpText;
	        textInHands.Add(foundText);
            if(hand.PlayerSoRef.DesiredTokenSide == TokenSide.White)
            {
                WhitePlayerText = foundText;
            }

            if (hand.PlayerSoRef.DesiredTokenSide == TokenSide.Black)
            {
                BlackPlayerText = foundText;
            }
	    }


	    _texts = textInHands.ToArray();
	}

    public void PlayMessage(string name)
    {
        AudioClip clip = SoundPlayer.Instance.AllSounds.Find(x => x.name == name);

        SoundPlayer.Instance.PlaySound(name);
        //Debug.Log("Playing: " + clip.name);
        StopCoroutine("DisplayText");
        StartCoroutine(DisplayText(clip.name, clip.length * 0.9f));

    }

    private IEnumerator DisplayText(string words, float duration)
    {

        if (words != "PlaceCard" && words != "SwapCards")
        {
            foreach (TextMesh textMesh in _texts)
            {
                textMesh.gameObject.SetActive(true);
                textMesh.renderer.enabled = true;
                textMesh.text = words;
            }

            yield return new WaitForSeconds(duration); //need to be realtime
            foreach (TextMesh textMesh in _texts)
            {
                textMesh.gameObject.SetActive(false);
                textMesh.renderer.enabled = false;
                textMesh.text = "";
            }
        }
        else
        {
            yield return null;
        }
    }

    public IEnumerator PlayMessageCoroutine(string name)
    {

        AudioClip clip = SoundPlayer.Instance.AllSounds.Find(x => x.name == name);
        Debug.Log("Playing: " + name);
        StopCoroutine("DisplayText");
        StartCoroutine(DisplayText(clip.name, clip.length * 0.9f));
        yield return StartCoroutine( SoundPlayer.Instance.PlaySoundCoroutine(name) );

    }
}
