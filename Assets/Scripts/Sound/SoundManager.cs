using UnityEngine;
using System;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public Sound[] sounds;
    public Sound[] music;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        SetAllSounds(sounds);
        SetAllSounds(music);
    }

    void SetAllSounds(Sound[] array)
    {
        foreach (Sound s in array)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    //Play specified sound effect
    public void PlaySoundEffect(string name)
    {
        PlaySound(name, sounds);
    }

    //Play specified music
    public void PlayMusic(string name)
    {
        //Find sound which has name
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("No Sound found called: " + name);
            return;
        }
        if (!s.source.isPlaying)
            s.source.Play();

    }

    //Stop a sound effect playing
    public void StopSoundEffect(string name)
    {
        //Find sound which has name
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("No Sound found called: " + name);
            return;
        }
        s.source.Stop();
    }

    //Stop a song playing
    public void StopMusic(string name)
    {
        //Find sound which has name
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("No Music found called: " + name);
            return;
        }
        s.source.Stop();
    }

    //Search for a sound to play
    void PlaySound(string name, Sound[] sounds)
    {
        //Find sound which has name
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("No Sound found called: " + name);
            return;
        }
        if (!s.source.isPlaying)
            s.source.Play();
    }


}

