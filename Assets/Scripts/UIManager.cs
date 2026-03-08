using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public TMP_Text healthText;
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public Slider xpSlider;

    [Header("Combo UI")]
    public TMP_Text comboText;
    public TMP_Text multiplierText;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text wpmText;
    public Button restartButton;

    private float _lastMultiplier;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (ProgressionManager.Instance == null) return;

        healthText.text = $"HP: {PlayerController.Instance.GetCurrentHealth()}";
        scoreText.text = $"SCORE: {ProgressionManager.Instance.totalScore}";
        levelText.text = $"LVL: {ProgressionManager.Instance.currentLevel}";
        
        xpSlider.maxValue = ProgressionManager.Instance.xpToNextLevel;
        xpSlider.value = ProgressionManager.Instance.currentXP;

        // Обновление комбо и множителя
        comboText.text = $"COMBO: {ProgressionManager.Instance.correctCharsInRow}";
        multiplierText.text = $"x{ProgressionManager.Instance.scoreMultiplier:F1}";

        // Эффект пульсации при изменении множителя
        if (ProgressionManager.Instance.scoreMultiplier > _lastMultiplier)
        {
            StopAllCoroutines();
            StartCoroutine(PulseEffect(multiplierText.transform));
        }
        _lastMultiplier = ProgressionManager.Instance.scoreMultiplier;
    }

    System.Collections.IEnumerator PulseEffect(Transform target)
    {
        Vector3 originalScale = Vector3.one;
        target.localScale = originalScale * 1.3f;
        float elapsed = 0;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, elapsed / 0.2f);
            yield return null;
        }
        target.localScale = originalScale;
    }

    public void ShowGameOver(int score)
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        finalScoreText.text = $"FINAL SCORE: {score}";
        wpmText.text = $"WPM: {ProgressionManager.Instance.GetWPM():F1}";
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}