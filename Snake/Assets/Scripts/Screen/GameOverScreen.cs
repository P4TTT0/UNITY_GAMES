using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public void Setup(string text)
    {
        this.gameObject.SetActive(true);
        this.pointsText.text = text;    
    }
}