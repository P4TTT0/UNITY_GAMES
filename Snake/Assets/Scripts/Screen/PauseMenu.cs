using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject PausePanel;

    public void Pause()
    {
        this.PausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        this.PausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void Menu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
