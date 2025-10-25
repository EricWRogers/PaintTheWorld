using UnityEngine;
using System; 

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip; 
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;  // AudioSource for background music
    public AudioSource sfxSource;    // AudioSource for sound effects

    [Header("Audio Clip Lists")]
    public Sound[] musicList;
    public Sound[] sfxList;

    void Awake()
    {
        if (instance == null)
        {
            // This is the first and only instance
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
            return;
        }

    }


    public void PlayMusic(string name)
    {
        // Find the sound in the musicList by name
        Sound s = Array.Find(musicList, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }

        // Set the clip, make it loop, and play
        musicSource.clip = s.clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        // Find the sound in the sfxList by name
        Sound s = Array.Find(sfxList, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("SFX: " + name + " not found!");
            return;
        }

        // Play the clip as a one-shot (allows overlapping sounds)
        sfxSource.PlayOneShot(s.clip);
    }
}