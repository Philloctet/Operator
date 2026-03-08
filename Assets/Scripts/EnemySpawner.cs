using UnityEngine;
using System.Collections.Generic;

// Класс для настройки каждого типа врага в инспекторе
[System.Serializable]
public class EnemySpawnData
{
    public string name;               // Просто для удобства в инспекторе
    public GameObject prefab;         // Ссылка на префаб
    public float baseWeight = 10f;    // Базовый шанс появления
    public float weightGrowthPerMinute = 0f; // На сколько увеличивается вес каждую минуту
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Types")]
    public List<EnemySpawnData> enemies;

    [Header("Spawn Settings")]
    public float spawnRadius = 12f; 
    public float initialTimeBetweenSpawns = 3f;
    public float minSpawnTime = 0.8f;
    
    // Насколько быстро уменьшается время между спавнами (в секундах за каждую минуту игры)
    public float difficultyRampPerMinute = 0.5f; 

    private float _timer;
    private float _gameStartTime;

    void Start()
    {
        _gameStartTime = Time.time;
    }

    void Update()
    {
        if (Time.timeScale <= 0) return; // Пауза

        _timer += Time.deltaTime;

        if (_timer >= GetCurrentSpawnInterval())
        {
            // Проверяем, есть ли свободные слова, прежде чем спавнить
            if (WordProvider.Instance != null && WordProvider.Instance.HasFreeWords(WordType.Enemy))
            {
                SpawnWeightedEnemy();
                _timer = 0;
            }
        }
    }

    private void SpawnWeightedEnemy()
    {
        if (PlayerController.Instance == null || enemies.Count == 0) return;

        float timePassedMinutes = (Time.time - _gameStartTime) / 60f;
        float totalWeight = 0f;

        // 1. Считаем сумму всех текущих весов
        foreach (var enemyData in enemies)
        {
            float currentWeight = enemyData.baseWeight + (enemyData.weightGrowthPerMinute * timePassedMinutes);
            totalWeight += Mathf.Max(0, currentWeight); // Вес не может быть отрицательным
        }

        if (totalWeight <= 0) return;

        // 2. Бросаем "рулетку"
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;
        GameObject selectedPrefab = null;

        foreach (var enemyData in enemies)
        {
            float currentWeight = enemyData.baseWeight + (enemyData.weightGrowthPerMinute * timePassedMinutes);
            cumulativeWeight += Mathf.Max(0, currentWeight);
            
            if (randomValue <= cumulativeWeight)
            {
                selectedPrefab = enemyData.prefab;
                break;
            }
        }

        // 3. Спавним выбранного врага
        if (selectedPrefab != null)
        {
            Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPosition = new Vector3(randomPoint.x, randomPoint.y, 0) + PlayerController.Instance.transform.position;
            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // Метод, который плавно уменьшает интервал спавна со временем
    private float GetCurrentSpawnInterval()
    {
        float timePassedMinutes = (Time.time - _gameStartTime) / 60f;
        float currentInterval = initialTimeBetweenSpawns - (difficultyRampPerMinute * timePassedMinutes);
        return Mathf.Max(minSpawnTime, currentInterval);
    }
}