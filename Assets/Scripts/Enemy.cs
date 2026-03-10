using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : MonoBehaviour, ITypable
{
    [Header("Stats")]
    public int maxHp = 20;
    protected int _currentHp;
    public float moveSpeed = 1.5f;
    public int xpReward = 20;
    public int scoreReward = 100; 
    protected float _freezeTimer = 0f;

    [Header("UI & Visuals")]
    [SerializeField] protected TMP_Text wordDisplay;
    [SerializeField] protected HealthBar healthBar;          // Ссылка на Хелсбар
    [SerializeField] protected GameObject damagePopupPrefab; // Префаб цифр урона
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color highlightColor = Color.red;

    protected string _currentWord;
    protected Rigidbody2D _rb;
    protected bool _isDying = false; 

    protected virtual void Start()
    {
        _currentHp = maxHp;
        _rb = GetComponent<Rigidbody2D>();
        
        // ПУНКТ 1: Изначально выключаем хелсбар
        if (healthBar != null) healthBar.Hide();
        
        GenerateNewWord();
        TypingManager.Instance.RegisterTypable(this);
    }

    protected virtual void FixedUpdate()
    {
        if (_isDying || PlayerController.Instance == null || PlayerController.Instance.IsDead()) return;

        // Логика заморозки ЭМИ
        if (_freezeTimer > 0)
        {
            _freezeTimer -= Time.fixedDeltaTime;
            return; // Пропускаем логику движения (враг стоит на месте)
        }

        // Логика движения
        Vector2 playerPos = PlayerController.Instance.transform.position;
        Vector2 newPos = Vector2.MoveTowards(transform.position, playerPos, moveSpeed * Time.fixedDeltaTime);
        
        // Применяем позицию к физическому телу (ЭТИ СТРОКИ СКОРЕЕ ВСЕГО ПОТЕРЯЛИСЬ)
        if (_rb != null) _rb.MovePosition(newPos);
        else transform.position = newPos;

        // Проверка на столкновение с игроком
        if (Vector2.Distance(transform.position, playerPos) < 0.5f)
        {
            PlayerController.Instance.TakeDamage(1);
            InstantDie(); 
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
    
    public void Freeze(float duration)
    {
        _freezeTimer = duration;
    }

    public Transform GetTransform() => transform;
}