using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Добавлен новый тип: Interactable
public enum WordType { Enemy, Navigation, Upgrade, Interactable }

public class WordProvider : MonoBehaviour
{
    public static WordProvider Instance;

    [Header("Dictionaries (Text Files)")]
    [SerializeField] private TextAsset enemyWordsFile;
    [SerializeField] private TextAsset navWordsFile;
    [SerializeField] private TextAsset upgradeWordsFile;
    [SerializeField] private TextAsset interactableWordsFile; // НОВОЕ ПОЛЕ для 5+ букв

    private List<string> _enemyPool = new List<string>();
    private List<string> _navPool = new List<string>();
    private List<string> _upgradePool = new List<string>();
    private List<string> _interactablePool = new List<string>(); // Новый пул для строений

    private HashSet<string> _activeWords = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadAllPools();
    }

    private void LoadAllPools()
    {
        LoadPool(_enemyPool, enemyWordsFile, "Enemy");
        LoadPool(_navPool, navWordsFile, "Navigation");
        LoadPool(_upgradePool, upgradeWordsFile, "Upgrade");
        LoadPool(_interactablePool, interactableWordsFile, "Interactable"); // Загрузка новых слов
    }

    private void LoadPool(List<string> pool, TextAsset file, string poolName)
    {
        if (file == null) return;
        string[] lines = file.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string trimmed = line.Trim().ToUpper();
            if (!string.IsNullOrEmpty(trimmed)) pool.Add(trimmed);
        }
        Debug.Log($"WordProvider: {poolName} pool loaded with {pool.Count} words.");
    }

    public bool HasFreeWords(WordType type)
    {
        List<string> targetPool = GetPoolByType(type);
        // Считаем, сколько слов из этого пула сейчас занято
        int busyCount = _activeWords.Count(w => targetPool.Contains(w));
        return busyCount < targetPool.Count;
    }

    public string GetUniqueWord(WordType type)
    {
        List<string> targetPool = GetPoolByType(type);
        if (targetPool.Count == 0) return null;

        // Ищем все слова в пуле, которые сейчас НЕ используются
        var availableWords = targetPool.Where(w => !_activeWords.Contains(w)).ToList();

        if (availableWords.Count > 0)
        {
            string chosen = availableWords[Random.Range(0, availableWords.Count)];
            _activeWords.Add(chosen);
            return chosen;
        }

        return null; 
    }

    public void ReleaseWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return;
        _activeWords.Remove(word);
    }

    private List<string> GetPoolByType(WordType type)
    {
        return type switch
        {
            WordType.Enemy => _enemyPool,
            WordType.Navigation => _navPool,
            WordType.Upgrade => _upgradePool,
            WordType.Interactable => _interactablePool, // Возвращаем пул строений
            _ => _enemyPool
        };
    }
}