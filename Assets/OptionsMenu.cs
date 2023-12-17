using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{

    public Slider volumeSlider;
    private MusicManager musicManager;

    void Start()
    {
        musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
        {
            // Set the slider to the current volume
            volumeSlider.value = musicManager.GetVolume();
        }

        // Attach the volume change handler
        volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChange(); });
    }

    public void OnVolumeChange()
    {
        if (musicManager != null)
        {
            musicManager.SetVolume(volumeSlider.value);
        }
    }
}

