using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [Header("Components")]
    public AudioMixer mainMixer;
    public Slider volumeSlider;

    [Header("Mixer Parameter")]
    public string exposedParameterName;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(exposedParameterName, 1f);

        volumeSlider.value = savedVolume;
        
        SetVolume(savedVolume);
    }

    public void SetVolume(float sliderValue)
    {
        float volumeInDecibels = Mathf.Log10(sliderValue) * 20;

        mainMixer.SetFloat(exposedParameterName, volumeInDecibels);
        
        PlayerPrefs.SetFloat(exposedParameterName, sliderValue);
    }
}