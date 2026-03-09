using UnityEngine;
using TMPro;
using System.Collections;

public class Turret : MonoBehaviour, ITypable
{
    [Header("Settings")]
    public float activeDuration = 10f;
    public float fireRate = 0.5f;
    public float attackRange = 10f;
    
    [Header("UI")]
    [SerializeField] private TMP_Text wordDisplay;
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color highlightColor = Color.yellow;

    [HideInInspector] public bool isActive = false;
    private string _currentWord;
    private Node _myNode;

    public void Setup(Node node)
    {
        _myNode = node;
        _myNode.currentBuilding = this;
        if (wordDisplay != null) wordDisplay.gameObject.SetActive(false);
    }

    public void EnableTyping()
    {
        if (isActive) return;
        
        // Используем пул слов Upgrade (т.к. это интерактивный объект)
        _currentWord = WordProvider.Instance.GetUniqueWord(WordType.Upgrade);
        if (wordDisplay != null)
        {
            wordDisplay.gameObject.SetActive(true);
            wordDisplay.text = _currentWord;
            wordDisplay.color = normalColor;
        }
        TypingManager.Instance.RegisterTypable(this);
    }

    public void DisableTyping()
    {
        if (isActive) return;

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
        DisableTyping(); // Убираем слово из системы
        
        isActive = true;
        StartCoroutine(ShootingRoutine());
    }

    public Transform GetTransform() => transform;

    // --- Логика Стрельбы ---
    private IEnumerator ShootingRoutine()
    {
        float timer = activeDuration;

        while (timer > 0)
        {
            ShootNearestEnemy();
            yield return new WaitForSeconds(fireRate);
            timer -= fireRate;
        }

        // Время вышло - уничтожаемся
        _myNode.currentBuilding = null;
        TurretManager.Instance.StartCooldown(); // Говорим менеджеру начать отсчет
        Destroy(gameObject);
    }

    private void ShootNearestEnemy()
    {
        // Ищем врагов в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.GetComponent<Enemy>() != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            // Создаем снаряд
            GameObject projPrefab = PlayerController.Instance.projectilePrefab;
            GameObject projGo = Instantiate(projPrefab, transform.position, Quaternion.identity);
            Projectile proj = projGo.GetComponent<Projectile>();

            if (proj != null)
            {
                // Турель использует статы игрока, но без мультишота и критов для баланса
                proj.damage = PlayerController.Instance.minDamage; 
                proj.speed = PlayerController.Instance.projectileSpeed * 1.5f;
                proj.pierceCount = 0;
                proj.Launch(closestEnemy.position, 0f);
            }
        }
    }
}