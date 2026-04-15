using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;
using SuperPupSystems.Helper;

public class AudioManager : MonoBehaviour
{   
    //Volume
    public AudioMixerGroup masterMixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup soundEffectMixer;
    private float masterVolume;
    private float musicVolume;
    private float soundEffectVolume;

    public Sound[] sounds;

    public static AudioManager instance;
    

    public string StartMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundEffectVolume = PlayerPrefs.GetFloat("SoundEffectVolume", 1f);
        masterMixer.audioMixer.SetFloat("MasterVolume", masterVolume);
        musicMixer.audioMixer.SetFloat("MusicVolume", musicVolume);
        soundEffectMixer.audioMixer.SetFloat("SFXVolume", soundEffectVolume);
    }

    void Start()
    {
        if (StartMusic != "")
        {
            PlayMusic(StartMusic);
        }

    }

    public void ChangeVolume(float volume, string mixer)
    {
        switch (mixer)
        {
            case "MasterVolume":
                masterMixer.audioMixer.SetFloat("MasterVolume", volume);
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
                break;
            case "MusicVolume":
                musicMixer.audioMixer.SetFloat("MusicVolume", volume);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                break;
            case "SoundEffectVolume":
                soundEffectMixer.audioMixer.SetFloat("SFXVolume", volume);
                PlayerPrefs.SetFloat("SoundEffectVolume", soundEffectVolume);
                break;
        }
    }

    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound != null)
        {
            AudioSource audioSource = SimpleObjectPool.instance.SpawnFromPool("AudioSource", transform.position, Quaternion.identity).GetComponent<AudioSource>();
            audioSource.gameObject.SetActive(true);
            sound.audioSource = audioSource;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.bypassEffects = sound.bypassEffects;
            sound.audioSource.bypassReverbZones = sound.bypassReverbZones;
            sound.audioSource.reverbZoneMix = sound.revebZoneMix;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.dopplerLevel = sound.dopplerLevel;
            //0 spactial blend means the sound is 2D and will play equally everywhere
            sound.audioSource.spatialBlend = 0f;
            if (sound.type == Sound.TypeOfSound.Music)
            {
                audioSource.outputAudioMixerGroup = musicMixer;
            }
            else
            {
                audioSource.outputAudioMixerGroup = soundEffectMixer;
            }
            sound.audioSource.Play();
            StartCoroutine(SoundFinished(audioSource));
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    public void PlayMusic(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound != null)
        {
            
            AudioSource audioSource = SimpleObjectPool.instance.SpawnFromPool("AudioSource", transform.position, Quaternion.identity).GetComponent<AudioSource>();
            if(sound.audioSource == null)
            {
               Debug.Log("DIDNT GET AUDIO SORUCE FROM POOL");
            }
            audioSource.gameObject.SetActive(true);
            sound.audioSource = audioSource;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = true;
            if (sound.type == Sound.TypeOfSound.Music)
            {
                audioSource.outputAudioMixerGroup = musicMixer;
            }
            else
            {
                audioSource.outputAudioMixerGroup = soundEffectMixer;
            }
            sound.audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    public void PlayAtPosition(string name, Vector3 position)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound != null)
        {
            AudioSource audioSource = SimpleObjectPool.instance.SpawnFromPool("AudioSource", position, Quaternion.identity).GetComponent<AudioSource>();
            audioSource.gameObject.SetActive(true);
            audioSource.transform.position = position;
            sound.audioSource = audioSource;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.bypassEffects = sound.bypassEffects;
            sound.audioSource.bypassReverbZones = sound.bypassReverbZones;
            sound.audioSource.reverbZoneMix = sound.revebZoneMix;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.dopplerLevel = sound.dopplerLevel;
            //1 spactial blend means the sound is 3D and will play louder when closer
            sound.audioSource.spatialBlend = 1f;
            sound.audioSource.rolloffMode = AudioRolloffMode.Custom;
            if (sound.type == Sound.TypeOfSound.Music)
            {
                audioSource.outputAudioMixerGroup = musicMixer;
            }
            else
            {
                audioSource.outputAudioMixerGroup = soundEffectMixer;
            }
            sound.audioSource.Play();
            StartCoroutine(SoundFinished(audioSource));
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    public void PlayOnObject(string name, GameObject audioObject)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        AudioSource audioSource;
        if (sound != null)
        {
            GameObject pooledObject = SimpleObjectPool.instance
            .SpawnFromPool("AudioSource", transform.position, Quaternion.identity);
            if (pooledObject != null)
            {
                audioSource = pooledObject.GetComponent<AudioSource>();
            }
            else
            {
                Debug.Log("There were no pooled AudioSources to play the sound with, don't forget to set them back inactive when they are done playing");
                return;
            }
            audioSource.gameObject.SetActive(true);
            audioSource.transform.SetParent(audioObject.transform);
            sound.audioSource = audioSource;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.bypassEffects = sound.bypassEffects;
            sound.audioSource.bypassReverbZones = sound.bypassReverbZones;
            sound.audioSource.reverbZoneMix = sound.revebZoneMix;
            sound.audioSource.maxDistance = sound.maxDistance;
            sound.audioSource.minDistance = sound.minDistance;
            sound.audioSource.dopplerLevel = sound.dopplerLevel;
            //1 spactial blend means the sound is 3D and will play louder when closer
            sound.audioSource.spatialBlend = 1f;
            sound.audioSource.rolloffMode = AudioRolloffMode.Custom;
            if (sound.type == Sound.TypeOfSound.Music)
            {
                audioSource.outputAudioMixerGroup = musicMixer;
            }
            else
            {
                audioSource.outputAudioMixerGroup = soundEffectMixer;
            }
            sound.audioSource.transform.position = sound.audioSource.transform.parent.position;
            sound.audioSource.Play();
            StartCoroutine(SoundFinished(audioSource));
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    private IEnumerator SoundFinished(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        //If the audioSource is parented to something else, set it back to the AudioManager
        audioSource.transform.SetParent(transform);
        audioSource.gameObject.SetActive(false);
    }

    //This is called just before loading into a new scene to gather all outstanding sounds back to the Singleton AudioManager so it doesn't lose them
    //public void GatherAllSounds()
    //{
    //    foreach (GameObject audioSourceObject in objectPool.pooledObjects)
    //    {
    //        if (audioSourceObject.transform.parent != gameObject)
    //        {
    //            audioSourceObject.transform.SetParent(transform);
    //            audioSourceObject.gameObject.SetActive(false);
    //        }
    //    }
    //}

    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound != null)
        {
            if (sound.audioSource != null)
            {
                sound.audioSource.Stop();
                sound.audioSource.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Sound not playing");
            }
        }
        else
        {
            Debug.LogWarning("Sound " + name + " was not found");
        }
    }

    //public void StopAll()
    //{
    //    foreach (GameObject audioSourceObject in objectPool.)
    //    {
    //        if (audioSourceObject.activeSelf)
    //        {
    //            audioSourceObject.SetActive(false);
    //        }
    //    }
    //}

    public void OnDestroy()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SoundEffectVolume", soundEffectVolume);        
    }
}
