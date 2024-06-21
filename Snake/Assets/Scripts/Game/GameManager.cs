using Assets.Scripts.Settings;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private Tilemap woodBounds;
    public GameOverScreen gameOverScreen;
    public int Score
    {
        get { return this.score; }
        set 
        { 
            this.score = value;
            this.pointsText.text = $"{this.Score} POINTS";
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
        this.AdjustVolume();
        this.AdjustDifficulty();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !this.snake.IsAlive) this.ResetGame();
        else if (Input.GetKeyDown(KeyCode.Escape)) this.PauseGame();
    }

    private void ResetGame()
    {
        this.snake.InitSnake();
        this.food.RandomizePosition();
        this.gameOverScreen.gameObject.SetActive(false);
        this.pointsText.gameObject.SetActive(true);
        this.Score = 0;
    }

    private void PauseGame()
    {
        if (!this.snake.IsAlive) return;
        this.pauseMenu.Pause();
    }

    public void GameOver()
    {
        this.gameOverScreen.Setup(this.Score);
        this.pointsText.gameObject.SetActive(false);
    }

    private void AdjustVolume()
    {
        var allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.volume = GameSettings.volume;
        }
    }

    private void AdjustDifficulty()
    {
        this.woodBounds.gameObject.SetActive(GameSettings.difficultyType == DifficultyType.Easy ? false : true);
    }
}
