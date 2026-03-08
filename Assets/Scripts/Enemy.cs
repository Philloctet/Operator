using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : MonoBehaviour, ITypable
{
    [Header("Stats")]
    public int maxHp = 20;
    private int _currentHp;
    public float moveSpeed = 1.5f;
    public int xpReward = 20;
    public int scoreReward = 100;

    [Header("UI & Visuals")]
    [SerializeField] private TMP_Text wordDisplay;
    [SerializeField] private HealthBar healthBar;          // Ссылка на Хелсбар
    [SerializeField] private GameObject damagePopupPrefab; // Префаб цифр урона
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.red;

    private string _currentWord;
    private Rigidbody2D _rb;
    private bool _isDying = false; 

    void Start()
    {
        _currentHp = maxHp;
        _rb = GetComponent<Rigidbody2D>();
        
        // ПУНКТ 1: Изначально выключаем хелсбар
        if (healthBar != null) healthBar.Hide();
        
        GenerateNewWord();
        TypingManager.Instance.RegisterTypable(this);
    }

    void FixedUpdate()
    {
        if (!_isDying && PlayerController.Instance != null && !PlayerController.Instance.IsDead())
        {
            Vector2 playerPos = PlayerController.Instance.transform.position;
            Vector2 newPos = Vector2.MoveTowards(transform.position, playerPos, moveSpeed * Time.fixedDeltaTime);
            
            if (_rb != null) _rb.MovePosition(newPos);
            else transform.position = newPos;

            if (Vector2.Distance(transform.position, playerPos) < 0.5f)
            {
                PlayerController.Instance.TakeDamage(1);
                InstantDie(); 
            }
        }
    }

    // ВЫЗЫВАЕТСЯ ИЗ PROJECTILE ПРИ ПОПАДАНИИ
    public void TakeDamage(int amount, bool isCrit)
    {
        if (_isDying) return;

        _currentHp -= amount;

        // ПУНКТ 1: Включаем и обновляем хелсбар при уроне
        if (healthBar != null)
        {
            healthBar.UpdateHealth(_currentHp, maxHp);
        }

        // ПУНКТ 2: Создаем вылетающие цифры урона
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            if (popup.TryGetComponent<DamagePopup>(out var popupScript))
            {
                popupScript.Setup(amount, isCrit);
            }
        }

        // Если ХП кончилось — проверяем, убить сразу или сделать фантомом
        if (_currentHp <= 0) 
        {
            // ПУНКТ 1: Убираем хелсбар, так как враг умер
            if (healthBar != null) healthBar.Hide();
            
            if (IsBeingTypedByPlayer()) EnterPhantomMode();
            else InstantDie();
        }
    }

    private void EnterPhantomMode()
    {
        _isDying = true;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers) r.enabled = false;

        StartCoroutine(PhantomFocusRoutine());
    }

    private IEnumerator PhantomFocusRoutine()
    {
        while (_isDying && IsBeingTypedByPlayer())
        {
            yield return null;
        }
        if (this != null) InstantDie();
    }

    private void InstantDie()
    {
        StopAllCoroutines();
        if (ProgressionManager.Instance != null && !_isDying) 
        {
            ProgressionManager.Instance.AddXP(xpReward);
            ProgressionManager.Instance.AddScore(scoreReward);
        }
        
        TypingManager.Instance.UnregisterTypable(this);
        WordProvider.Instance.ReleaseWord(_currentWord);
        Destroy(gameObject);
    }

    private void GenerateNewWord()
    {
        if (!string.IsNullOrEmpty(_currentWord))
            WordProvider.Instance.ReleaseWord(_currentWord);

        _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Enemy);
        if (wordDisplay != null) 
        {
            wordDisplay.text = _currentWord;
            wordDisplay.color = normalColor;
        }
    }

    private bool IsBeingTypedByPlayer()
    {
        string buffer = TypingManager.Instance.GetCurrentBuffer();
        return !string.IsNullOrEmpty(buffer) && _currentWord.StartsWith(buffer);
    }

    // --- ITypable ---
    public string GetWord() => _currentWord;

    public void OnCharTyped(int index)
    {
        if (wordDisplay == null) return;
        string typed = _currentWord.Substring(0, index);
        string remaining = _currentWord.Substring(index);
        wordDisplay.text = $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{typed}</color>{remaining}";
    }

    public void OnReset()
    {
        if (wordDisplay != null)
        {
            wordDisplay.text = _currentWord;
            wordDisplay.color = normalColor;
        }
    }

    public void OnComplete()
    {
        ProgressionManager.Instance.RegisterCompletedWord(_currentWord);
        PlayerController.Instance.FireAt(this);

        if (!_isDying) GenerateNewWord();
        else InstantDie();
    }

    public Transform GetTransform() => transform;
}