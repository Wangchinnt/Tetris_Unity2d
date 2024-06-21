/* This class for manage the UI element in Setting Menu*/
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [SerializeField] Button toggleMusicButton;
    [SerializeField] Button toggleSoundsButton;
    [SerializeField] Text musicStatusText;
    [SerializeField] Text soundsStatusText;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        // Register button click events
        toggleMusicButton.onClick.AddListener(ToggleMusic);
        toggleSoundsButton.onClick.AddListener(ToggleSounds);

        // Update initial status
        UpdateMusicStatus();
        UpdateSoundsStatus();
    }

    private void ToggleMusic()
    {
        if (audioManager != null)
        {
            audioManager.ToggleMusic();
            UpdateMusicStatus();
        }
    }

    private void ToggleSounds()
    {
        if (audioManager != null)
        {
            audioManager.ToggleSounds();
            UpdateSoundsStatus();
        }
    }

    private void UpdateMusicStatus()
    {
        if (audioManager != null)
        {
            if (audioManager.IsMusicOn())
            {
                musicStatusText.text = "On";
            }
            else
            {
                musicStatusText.text = "Off";
            }
        }
    }

    private void UpdateSoundsStatus()
    {
        if (audioManager != null)
        {
            if (audioManager.AreSoundsOn())
            {
                soundsStatusText.text = "On";
            }
            else
            {
                soundsStatusText.text = "Off";
            }
        }
    }
}
