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
        
        _source.PlayOneShot(clip);
    }

}
