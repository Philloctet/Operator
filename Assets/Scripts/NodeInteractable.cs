using UnityEngine;
using TMPro;

public abstract class NodeInteractable : MonoBehaviour, ITypable
{
    [Header("Interactable Settings")]
    public float lifetime = 120f; // Время до исчезновения (2 минуты по умолчанию)
    protected float _lifetimeTimer;

    [Header("UI")]
    [SerializeField] protected TMP_Text wordDisplay;
    [SerializeField] protected Color normalColor = Color.cyan;
    [SerializeField] protected Color highlightColor = Color.yellow;

    protected bool _isPlayerPresent = false;
    protected string _currentWord;
    public Node myNode; // Ссылка на ноду
    protected bool _isActivated = false;

    public virtual void Setup(Node node)
    {
        myNode = node;
        myNode.currentBuilding = this;
        _lifetimeTimer = lifetime;
        if (wordDisplay != null) wordDisplay.gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        if (_isActivated) return;

        // Таймер жизни объекта на карте
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0)
        {
            Despawn();
        }
    }

    public virtual void EnableTyping()
    {
        if (_isActivated) return;
        _isPlayerPresent = true;
        
        _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Interactable);
        if (wordDisplay != null)
        {
            wordDisplay.gameObject.SetActive(true);
            wordDisplay.text = _currentWord;
            wordDisplay.color = normalColor;
        }
        TypingManager.Instance.RegisterTypable(this);
    }

    public virtual void DisableTyping()
    {
        if (_isActivated || !_isPlayerPresent) return;
        _isPlayerPresent = false;

        TypingManager.Instance.UnregisterTypable(this);
        if (!string.IsNullOrEmpty(_currentWord)) WordProvider.Instance.ReleaseWord(_currentWord);
        
        if (wordDisplay != null) wordDisplay.gameObject.SetActive(false);
        _currentWord = "";
    }

    // --- Логика ITypable ---
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
        if (wordDisplay != null) wordDisplay.text = _currentWord;
    }

    public void OnComplete()
    {
        ProgressionManager.Instance.RegisterCompletedWord(_currentWord);
        
        // 1. СНАЧАЛА отписываемся от TypingManager и освобождаем слово
        DisableTyping(); 
        
        // 2. ТОЛЬКО ПОТОМ блокируем объект от повторных взаимодействий
        _isActivated = true; 
        
        // 3. Откладываем уничтожение до конца кадра
        StartCoroutine(ProcessRewardRoutine()); 
    }

    private System.Collections.IEnumerator ProcessRewardRoutine()
    {
        // Ждем самую малость - до конца текущего кадра
        yield return new WaitForEndOfFrame(); 
        
        ApplyReward(); 
    }

    public Transform GetTransform() => transform;

    // ЭТОТ МЕТОД БУДУТ ПЕРЕОПРЕДЕЛЯТЬ НАСЛЕДНИКИ (Сундук, Бонус и т.д.)
    protected abstract void ApplyReward();

    public virtual void Despawn()
    {
        if (_isPlayerPresent) DisableTyping();
        if (myNode != null) myNode.currentBuilding = null;
        Destroy(gameObject);
    }
}