using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public UnityEvent OnResyncedSettings; //Resynced from PlayerPrefs.

    [Header("Audio")]
    public AudioSettings audioSettings;
    public AudioMixer audioMixer;
    public Slider audioSlider;

    [Header("Notification")]
    public bool allowNotifications;
    public Button notificationButtonOn;
    public Button notificationButtonOff;

    [Header("Maximum FPS")]
    public Button button30FPS;
    public Button button60FPS;
    public Button button120FPS;

    [Header("Debug")]
    public bool allowDebug;

    [Header("Scripts")]
    public ColorUIButton colorUIButton;

    private void Start()
    {
        ResyncSettingsMenu();
    }

    public void ResyncSettingsMenu()
    {
        if(PlayerPrefs.HasKey("FrameCap"))
        {
            ResyncFrameCap();
        }
        if (PlayerPrefs.HasKey("MainMusic"))
        {
            ResyncAudioVolume();
        }
        if (PlayerPrefs.HasKey("InGameNotifications"))
        {
            ResyncInGameNotifications();
        }
        OnResyncedSettings.Invoke();
    }
    #region Frames
    public void SetFrameCap(int value)
    {
        PlayerPrefs.SetInt("FrameCap", value);
        Application.targetFrameRate = value;

        if (value == 30)
        {
            colorUIButton.SelectedButtonColor(button30FPS);
            colorUIButton.ResetButtonColor(button60FPS);
            colorUIButton.ResetButtonColor(button120FPS);
        }
        else if (value == 60)
        {
            colorUIButton.ResetButtonColor(button30FPS);
            colorUIButton.SelectedButtonColor(button60FPS);
            colorUIButton.ResetButtonColor(button120FPS);
        }
        else if (value == 120)
        {
            colorUIButton.ResetButtonColor(button30FPS);
            colorUIButton.ResetButtonColor(button60FPS);
            colorUIButton.SelectedButtonColor(button120FPS);
        }
    }
    private void ResyncFrameCap()
    {
        int value;
        value = PlayerPrefs.GetInt("FrameCap");
        
        if(value == 30)
        {
            colorUIButton.SelectedButtonColor(button30FPS);
            colorUIButton.ResetButtonColor(button60FPS);
            colorUIButton.ResetButtonColor(button120FPS);
        }
        else if(value == 60)
        {
            colorUIButton.ResetButtonColor(button30FPS);
            colorUIButton.SelectedButtonColor(button60FPS);
            colorUIButton.ResetButtonColor(button120FPS);
        }
        else if(value == 120)
        {
            colorUIButton.ResetButtonColor(button30FPS);
            colorUIButton.ResetButtonColor(button60FPS);
            colorUIButton.SelectedButtonColor(button120FPS);
        }
    }

    #endregion

    #region Audio
    public void GetSliderVolume()
    {
        ChangeAudioVolume(audioSlider.value);
    }
    public void ChangeAudioVolume(float volume)
    {
        PlayerPrefs.SetFloat("MainMusic", volume);
        audioMixer.SetFloat("MainMusic", volume);
    }
    private void ResyncAudioVolume()
    {
        float volume;
        volume = PlayerPrefs.GetFloat("MainMusic");
        audioMixer.SetFloat("MainMusic", volume);
        if (audioSlider)
        {
            audioSlider.value = volume;
        }
    }
    #endregion

    #region In-game notifications

    public void ToggleInGameNotifications()
    {
        if(!notificationButtonOff || !notificationButtonOn && allowDebug)
        {
            Debug.LogWarning("SettingsMenu is missing button variables!", this);
            return;
        }

        if (allowNotifications)
        {
            colorUIButton.SelectedButtonColor(notificationButtonOff);
            colorUIButton.ResetButtonColor(notificationButtonOn);
            PlayerPrefs.SetInt("InGameNotifications", 0);
        }
        else
        {
            colorUIButton.SelectedButtonColor(notificationButtonOn);
            colorUIButton.ResetButtonColor(notificationButtonOff);
            PlayerPrefs.SetInt("InGameNotifications", 1);
        }
        allowNotifications = !allowNotifications;
        
        
    }
    private void ResyncInGameNotifications()
    {
        int notificationsInt = new();
        if(PlayerPrefs.GetInt("InGameNotifications", notificationsInt) == 0)
        {
            colorUIButton.SelectedButtonColor(notificationButtonOff);
            colorUIButton.ResetButtonColor(notificationButtonOn);
            allowNotifications = false;
        }
        else
        {
            colorUIButton.SelectedButtonColor(notificationButtonOn);
            colorUIButton.ResetButtonColor(notificationButtonOff);
            allowNotifications = true;
        }
    }

    #endregion
}
