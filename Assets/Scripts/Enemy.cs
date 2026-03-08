using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour, ITypable
{
    [Header("Stats")]
    public int hp = 1;
    public float moveSpeed = 1.5f;
    public int xpReward = 20;
    public int scoreReward = 100;

    [Header("UI")]
    [SerializeField] private TMP_Text wordDisplay;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.red;

    private string _currentWord;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        if (!SetNewWord())
        {
            Destroy(gameObject);
            return;
        }
        
        TypingManager.Instance.RegisterTypable(this);
        if (wordDisplay != null) wordDisplay.color = normalColor;
    }

    void FixedUpdate() // Используем FixedUpdate для стабильного движения
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.IsDead())
        {
            // Получаем позиции только в 2D (игнорируем Z)
            Vector2 currentPos = transform.position;
            Vector2 playerPos = PlayerController.Instance.transform.position;
            
            // Движение
            Vector2 newPos = Vector2.MoveTowards(currentPos, playerPos, moveSpeed * Time.fixedDeltaTime);
            
            if (_rb != null)
                _rb.MovePosition(newPos);
            else
                transform.position = newPos;

            // Проверка дистанции для атаки (0.5f — радиус контакта)
            if (Vector2.Distance(currentPos, playerPos) < 0.5f)
            {
                PlayerController.Instance.TakeDamage(1);
                Die(false); 
            }
        }
    }

    private bool SetNewWord()
    {
        if (!string.IsNullOrEmpty(_currentWord))
            WordProvider.Instance.ReleaseWord(_currentWord);

        _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Enemy);
        
        if (string.IsNullOrEmpty(_currentWord)) return false;

        if (wordDisplay != null) wordDisplay.text = _currentWord;
        return true;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) Die(true);
    }

    private void Die(bool killedByPlayer)
    {
        if (killedByPlayer && ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.AddXP(xpReward);
            ProgressionManager.Instance.AddScore(scoreReward);
        }
        
        TypingManager.Instance.UnregisterTypable(this);
        WordProvider.Instance.ReleaseWord(_currentWord);
        Destroy(gameObject);
    }

    public string GetWord() => _currentWord;

    public void OnCharTyped(int index)
    {
        if (string.IsNullOrEmpty(_currentWord) || wordDisplay == null) return;
        
        string typed = _currentWord.Substring(0, index);
        string remaining = _currentWord.Substring(index);
        wordDisplay.text = $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{typed}</color>{remaining}";
    }

    public void OnReset()
    {
        if (wordDisplay != null) wordDisplay.text = _currentWord;
    }

    public void OnComplete()
    {
        // Регистрируем завершенное слово для комбо и WPM
        ProgressionManager.Instance.RegisterCompletedWord(_currentWord);
    
        PlayerController.Instance.FireAt(this);
        if (!SetNewWord()) Die(false);
    }

    public Transform GetTransform() => transform;
}