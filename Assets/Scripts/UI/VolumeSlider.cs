using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{

    public string AudioMixerName;
    public Slider audioSlider;

    void Start()
    {
        if (AudioMixerName == "MasterVolume")
        {
            audioSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }
        else if (AudioMixerName == "MusicVolume")
        {
            audioSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
        else if (AudioMixerName == "SoundEffectVolume")
        {
            audioSlider.value = PlayerPrefs.GetFloat("SoundEffectVolume", 1f);
        }
        
        audioSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
    }

    public void OnSliderValueChanged()
    {
        float volume = Mathf.Log10(Mathf.Clamp(audioSlider.value, 0.0001f, 1f)) * 20f;
        AudioManager.instance.ChangeVolume(volume, AudioMixerName);
    }

    
}