using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private GameStats gameStats;
    [SerializeField] private Text livesText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text keyText;

    private void OnEnable()
    {
        if (gameStats != null) gameStats.OnStatsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (gameStats != null) gameStats.OnStatsChanged -= Refresh;
    }

    public void Configure(GameStats stats, Text lives, Text score, Text gold, Text key)
    {
        if (isActiveAndEnabled && gameStats != null) gameStats.OnStatsChanged -= Refresh;
        gameStats = stats;
        livesText = lives;
        scoreText = score;
        goldText = gold;
        keyText = key;
        if (isActiveAndEnabled && gameStats != null) gameStats.OnStatsChanged += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        if (gameStats == null) return;
        if (livesText != null) livesText.text = gameStats.Lives.ToString();
        if (scoreText != null) scoreText.text = gameStats.Score.ToString();
        if (goldText != null) goldText.text = gameStats.Gold.ToString();
        if (keyText != null) keyText.text = $"{gameStats.Keys}/{gameStats.RequiredKeys}";
    }
}
