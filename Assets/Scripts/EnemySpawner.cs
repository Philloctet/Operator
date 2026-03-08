using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 12f; 
    public float initialTimeBetweenSpawns = 3f;
    public float minSpawnTime = 0.8f;
    public float difficultyIncreaseRate = 0.02f; 

    private float _timer;
    private float _currentTimeBetweenSpawns;

    void Start()
    {
        _currentTimeBetweenSpawns = initialTimeBetweenSpawns;
    }

    void Update()
    {
        if (Time.timeScale <= 0) return; // Не спавним на паузе

        _timer += Time.deltaTime;

        if (_timer >= _currentTimeBetweenSpawns)
        {
            // ПРОВЕРКА: Есть ли свободные слова?
            if (WordProvider.Instance != null && WordProvider.Instance.HasFreeWords(WordType.Enemy))
            {
                SpawnEnemy();
                _timer = 0;
                
                // Постепенное ускорение спавна
                _currentTimeBetweenSpawns = Mathf.Max(minSpawnTime, _currentTimeBetweenSpawns - difficultyIncreaseRate);
            }
        }
    }

    void SpawnEnemy()
    {
        if (PlayerController.Instance == null) return;

        Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomPoint.x, randomPoint.y, 0) + PlayerController.Instance.transform.position;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}