using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SnakeScene");
    }

    public void QuitGame()
    {
        print("Has salido del juego.");
        Application.Quit();
    }
}