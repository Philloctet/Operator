using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 5f;
    private int _currentHealth;
    private bool _isDead = false;

    [Header("Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Combat Stats")]
    public int minDamage = 5;
    public int maxDamage = 7;
    [Range(0, 100)]
    public float critChance = 10f; // 10% шанс крита
    
    // Переменные для системы улучшений
    public int pierceCount = 0; 
    public int projectilesPerShot = 1;
    public float damageMultiplier = 1.0f;
    public float spreadAngle = 10f;
    public float projectileSpeed = 10f;

    [Header("Navigation")]
    public Node currentNode;
    private bool _isMoving = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        _currentHealth = maxHealth;

        if (currentNode != null)
        {
            transform.position = currentNode.transform.position;
            yield return null; 
            UpdateAvailableNodes();
        }
        else
        {
            Debug.LogError("PlayerController: Назначьте стартовую ноду в инспекторе!");
        }
    }

    void Update()
    {
        if (_isMoving && currentNode != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentNode.transform.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentNode.transform.position) < 0.01f)
            {
                transform.position = currentNode.transform.position;
                _isMoving = false;
                UpdateAvailableNodes();
            }
        }
    }

    public void FireAt(Enemy target)
    {
        if (target == null || _isDead) return;

        for (int i = 0; i < projectilesPerShot; i++)
        {
            GameObject projGo = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile proj = projGo.GetComponent<Projectile>();

            if (proj != null)
            {
                // Расчет урона
                int baseDamage = Random.Range(minDamage, maxDamage + 1);
                bool isCrit = Random.value * 100f < critChance;
                int finalDamage = isCrit ? baseDamage * 2 : baseDamage;

                proj.speed = projectileSpeed;
                proj.pierceCount = pierceCount;
            
                // Передаем урон и флаг крита в снаряд
                proj.damage = finalDamage;
                proj.isCrit = isCrit;

                if (projectilesPerShot > 1)
                {
                    float angleOffset = (i - (projectilesPerShot - 1) / 2f) * spreadAngle;
                    proj.transform.Rotate(0, 0, angleOffset);
                }

                proj.Launch(target.transform);
            }
        }
    }

    public void MoveToNode(Node targetNode)
    {
        if (_isMoving || _isDead) return;
        DeactivateAllNeighbors();
        currentNode = targetNode;
        _isMoving = true;
    }

    private void UpdateAvailableNodes()
    {
        if (currentNode == null || _isDead) return;
        foreach (Node neighbor in currentNode.neighbors)
        {
            if (neighbor != null) neighbor.SetTargetable(true);
        }
    }

    private void DeactivateAllNeighbors()
    {
        if (currentNode == null) return;
        foreach (Node neighbor in currentNode.neighbors)
        {
            if (neighbor != null) neighbor.SetTargetable(false);
        }
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        _currentHealth -= amount;
        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;
        if (UIManager.Instance != null && ProgressionManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(ProgressionManager.Instance.totalScore);
        }
    }

    public int GetCurrentHealth() => _currentHealth;
    public bool IsDead() => _isDead;
}