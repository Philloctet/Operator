using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [Header("Base Stats")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int totalScore = 0;

    [Header("Combo & Multiplier")]
    public int correctCharsInRow = 0;
    public float scoreMultiplier = 1.0f;
    [Tooltip("На сколько увеличивается множитель")]
    public float multiplierStep = 0.5f;
    [Tooltip("За сколько накопленных символов дается шаг множителя")]
    public int charsPerStep = 5;

    [Header("WPM Tracking")]
    private float _totalPlayTime = 0f;
    private int _completedObjectsCount = 0;

    [Header("UI References")]
    public GameObject upgradePanel;
    public List<UpgradeCardUI> upgradeCards;
    public GameObject levelUpHintText; 

    private bool _isLevelUpPending = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Time.timeScale > 0 && !_isLevelUpPending)
        {
            _totalPlayTime += Time.deltaTime;
        }

        if (_isLevelUpPending && Time.timeScale > 0)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ShowUpgradeMenu();
            }
        }
    }

    // ВАЖНО: Теперь это вызывается ТОЛЬКО когда слово закончено
    public void RegisterCompletedWord(string word)
    {
        if (Time.timeScale <= 0 || string.IsNullOrEmpty(word)) return;

        // Добавляем длину слова к общему счетчику комбо
        correctCharsInRow += word.Length;

        // Обновляем множитель (без ограничения сверху)
        scoreMultiplier = 1.0f + (correctCharsInRow / charsPerStep) * multiplierStep;
        
        // Также учитываем объект для WPM
        _completedObjectsCount++;
    }

    public void RegisterMistake()
    {
        if (Time.timeScale <= 0) return;

        correctCharsInRow = 0;
        scoreMultiplier = 1.0f;
    }

    public void AddScore(int baseAmount)
    {
        // Очки начисляются с учетом текущего множителя
        totalScore += Mathf.RoundToInt(baseAmount * scoreMultiplier);
    }

    public float GetWPM()
    {
        if (_totalPlayTime <= 0) return 0;
        float minutes = _totalPlayTime / 60f;
        return _completedObjectsCount / minutes;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel) 
        {
            _isLevelUpPending = true;
        
            // ВКЛЮЧАЕМ подсказку над игроком
            if (levelUpHintText != null)
                levelUpHintText.SetActive(true);
        }
    }

    private void ShowUpgradeMenu()
    {
        _isLevelUpPending = false;
        if (levelUpHintText != null) levelUpHintText.SetActive(false);

        Time.timeScale = 0f;
    
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        
            // ВКЛЮЧАЕМ режим меню в менеджере печати
            TypingManager.Instance.SetMenuMode(true); 

            foreach (var card in upgradeCards) card.SetupRandomUpgrade();
        }
    }

    public void CompleteUpgrade()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);

        // ВЫКЛЮЧАЕМ режим меню, возвращаемся к бою
        TypingManager.Instance.SetMenuMode(false); 

        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.2f);
        Time.timeScale = 1f;
    }
}