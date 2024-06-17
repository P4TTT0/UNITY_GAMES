using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameOverScreen gameOverScreen;
    public int Score
    {
        get { return this.score; }
        set 
        { 
            this.score = value;
            this.pointsText.text = $"{this.Score.ToString()} POINTS";
        }
    }
    public TextMeshProUGUI pointsText;
    private Snake snake;
    private Food food;
    private int score;

    void Start()
    {
        this.snake = FindObjectOfType<Snake>();
        this.food = FindObjectOfType<Food>();
        this.pointsText.text = "0 POINTS";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !this.snake.IsAlive) this.ResetGame();
        else if (Input.GetKeyDown(KeyCode.Escape)) this.BackToMainMenu();
    }

    private void ResetGame()
    {
        this.snake.InitSnake();
        this.food.RandomizePosition();
        this.gameOverScreen.gameObject.SetActive(false);
        this.pointsText.gameObject.SetActive(true);
        this.Score = 0;
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GameOver()
    {
        this.gameOverScreen.Setup(this.Score);
        this.pointsText.gameObject.SetActive(false);
    }
}
