using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{

    public string AudioMixerName;
    public Slider audioSlider;

    void Start()
    {
        float db = 0f;
        if (AudioMixerName == "MasterVolume")
        {
            db = PlayerPrefs.GetFloat("MasterVolume", -2.5f);
        }
        else if (AudioMixerName == "MusicVolume")
        {
            db = PlayerPrefs.GetFloat("MusicVolume", -2.5f);
        }
        else if (AudioMixerName == "SoundEffectVolume")
        {
            db = PlayerPrefs.GetFloat("SoundEffectVolume", -2.5f);
        }

        audioSlider.value = Mathf.Pow(10f, db / 20f);
        audioSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
    }

    public void OnSliderValueChanged()
    {
        float volume = Mathf.Log10(Mathf.Clamp(audioSlider.value, 0.001f, 1f)) * 20f;
        AudioManager.instance.ChangeVolume(volume, AudioMixerName);
    }

    
}