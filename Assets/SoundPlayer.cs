using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundPlayer : Singleton<SoundPlayer>
{

    private AudioSource _source;

    public List<AudioClip> AllSounds;
    public void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {
        AudioClip clip = AllSounds.Find(x => x.name == name);
        Debug.Log("Playing: " + name);
        _source.PlayOneShot(clip);
        //Debug.Log("Playing: " + clip.name);
    }

    public IEnumerator PlaySoundCoroutine(string name)
    {
        AudioClip clip = AllSounds.Find(x => x.name == name);
        Debug.Log("Playing: " + name);
        _source.PlayOneShot(clip);
        while (_source.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return null;
        //Debug.Log("Playing: " + clip.name);
    }

}
