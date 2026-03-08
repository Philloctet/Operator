using UnityEngine;
using TMPro;
using System.Collections;

public class Enemy : MonoBehaviour, ITypable
{
    [Header("Stats")]
    public float moveSpeed = 1.5f;
    public int xpReward = 20;
    public int scoreReward = 100;

    [Header("UI")]
    [SerializeField] private TMP_Text wordDisplay;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.red;

    private string _currentWord;
    private Rigidbody2D _rb;
    private bool _isDying = false; 

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        GenerateNewWord();
        TypingManager.Instance.RegisterTypable(this);
    }

    void FixedUpdate()
    {
        // Враг движется, только если он не "фантом"
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

    // Вызывается СНАРЯДОМ при попадании
    public void TakeHit()
    {
        if (_isDying) return;

        // Если игрок сейчас печатает СЛЕДУЮЩЕЕ слово этого врага
        if (IsBeingTypedByPlayer())
        {
            EnterPhantomMode();
        }
        else
        {
            InstantDie();
        }
    }

    private void EnterPhantomMode()
    {
        _isDying = true;
        // Отключаем физику и спрайты, но текст ОСТАВЛЯЕМ
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers) r.enabled = false;

        StartCoroutine(PhantomFocusRoutine());
    }

    private IEnumerator PhantomFocusRoutine()
    {
        // Пока игрок держит фокус на этом "фантомном" слове
        while (_isDying && IsBeingTypedByPlayer())
        {
            yield return null;
        }

        // Если передумал или закончил - удаляем окончательно
        if (this != null) InstantDie();
    }

    private void InstantDie()
    {
        StopAllCoroutines();
        if (ProgressionManager.Instance != null && !_isDying) // Если убит физически
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

    public void OnReset() => wordDisplay.text = _currentWord;

    public void OnComplete()
    {
        // 1. Считаем завершенное слово для комбо/WPM
        ProgressionManager.Instance.RegisterCompletedWord(_currentWord);
        
        // 2. Стреляем в текущую позицию врага (даже если он станет фантомом, пуля полетит туда)
        PlayerController.Instance.FireAt(this);

        // 3. Если враг еще жив (не стал фантомом), просто даем ему НОВОЕ слово
        if (!_isDying)
        {
            GenerateNewWord();
        }
        else
        {
            // Если мы уже были фантомом и допечатали слово - исчезаем
            InstantDie();
        }
    }

    public Transform GetTransform() => transform;
}