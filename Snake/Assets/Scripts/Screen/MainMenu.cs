using Assets.Scripts.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Slider volumeSlider;

    public void Start()
    {
        this.difficultyDropdown.onValueChanged.AddListener(delegate { OnDifficultyChange(this.difficultyDropdown); });

        this.volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChange(this.volumeSlider); });

        this.volumeSlider.value = GameSettings.volume;
        this.difficultyDropdown.value = Convert.ToInt32(GameSettings.difficultyType);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SnakeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDifficultyChange(TMP_Dropdown sender)
    {
        switch (sender.value)
        {
            case 0:
                GameSettings.difficultyType = DifficultyType.Easy;
                break;
            case 1:
                GameSettings.difficultyType = DifficultyType.Medium;
                break;
            default:
                GameSettings.difficultyType = DifficultyType.Hard;
                break;
        }
    }

    private void OnVolumeChange(Slider sender)
    {
        GameSettings.volume = sender.value;
    }
}
