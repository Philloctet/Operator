using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public Node currentNode;
    public float moveSpeed = 5f;
    
    [Header("Combat Stats")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public int baseDamage = 1;
    public int pierceCount = 0;
    public float damageMultiplier = 1f;
    public int projectilesPerShot = 1;

    [Header("Survival")]
    public int maxHealth = 3;
    private int _currentHealth;
    private bool _isDead = false;

    private bool _isMoving = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private IEnumerator Start() // Замени void на IEnumerator
    {
        _currentHealth = maxHealth;

        if (currentNode != null)
        {
            transform.position = currentNode.transform.position;
        
            // Ждем один кадр, чтобы все синглтоны (WordProvider, TypingManager) проснулись
            yield return null; 
        
            UpdateAvailableNodes();
        }
        else
        {
            Debug.LogError("PlayerController: Назначьте стартовую ноду!");
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        Debug.Log($"Игрок получил урон! HP: {_currentHealth}");

        // Здесь можно добавить эффект тряски камеры или вспышку экрана

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        Time.timeScale = 0f; 

        // Вызываем визуальное окно через UIManager
        if (UIManager.Instance != null && ProgressionManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(ProgressionManager.Instance.totalScore);
        }
    }

    public void MoveToNode(Node targetNode)
    {
        if (_isMoving || _isDead) return;
        StartCoroutine(MoveRoutine(targetNode));
    }

    private IEnumerator MoveRoutine(Node targetNode)
    {
        _isMoving = true;
        ClearAvailableNodes();

        Vector3 startPos = transform.position;
        Vector3 endPos = targetNode.transform.position;
        float progress = 0;

        while (progress < 1f)
        {
            progress += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        transform.position = endPos;
        currentNode = targetNode;
        _isMoving = false;
        UpdateAvailableNodes();
    }

    private void UpdateAvailableNodes()
    {
        if (currentNode == null || _isDead) return;
        foreach (var neighbor in currentNode.neighbors)
        {
            if (neighbor != null) neighbor.SetTargetable(true);
        }
    }

    private void ClearAvailableNodes()
    {
        if (currentNode == null) return;
        foreach (var neighbor in currentNode.neighbors)
        {
            if (neighbor != null) neighbor.SetTargetable(false);
        }
    }

    public void FireAt(Enemy target)
    {
        if (target == null || _isDead) return;

        if (projectilePrefab == null) return;

        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        for (int i = 0; i < projectilesPerShot; i++)
        {
            GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (projObj.TryGetComponent<Projectile>(out Projectile proj))
            {
                Vector3 direction = target.transform.position - transform.position;
                if (projectilesPerShot > 1)
                {
                    direction = Quaternion.Euler(0, 0, Random.Range(-15f, 15f)) * direction;
                }
                proj.Setup(direction, projectileSpeed, finalDamage, pierceCount);
            }
        }
    }

    public int GetCurrentHealth() => _currentHealth;
    public bool IsDead() => _isDead;
}