using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public void Setup(int score)
    {
        this.gameObject.SetActive(true);
        this.pointsText.text = $"{score} POINTS.";    
    }
}