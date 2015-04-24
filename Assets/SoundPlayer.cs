using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundPlayer : Singleton<SoundPlayer>
{

    private AudioSource _source;
    private TextMesh[] _texts;
    public List<AudioClip> AllSounds;
    public void Awake()
    {
        _source = GetComponent<AudioSource>();
        _texts = GetComponentsInChildren<TextMesh>();
    }

    public void PlaySound(string name)
    {
        AudioClip clip = AllSounds.Find(x => x.name == name);
        Debug.Log("Playing: " + name);
        _source.PlayOneShot(clip);
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
                textMesh.text = words;
            }

            yield return new WaitForSeconds(duration); //need to be realtime
            foreach (TextMesh textMesh in _texts)
            {
                textMesh.text = "";
            }
        }
        else
        {
            yield return null;
        }
    }

    public IEnumerator PlaySoundCoroutine(string name)
    {
        
        AudioClip clip = AllSounds.Find(x => x.name == name);
        Debug.Log("Playing: " + name);
        StopCoroutine("DisplayText");
        StartCoroutine(DisplayText(clip.name, clip.length * 0.9f));
        _source.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length * 0.8f);
        
        //Debug.Log("Playing: " + clip.name);
    }

}
