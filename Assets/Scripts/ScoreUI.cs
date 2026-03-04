using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    void Update()
    {
        if (GameManager.Instance != null && scoreText != null)
            scoreText.text = "Score: " + GameManager.Instance.score;
    }
}
