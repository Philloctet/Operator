using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Stats")]
    public int maxHealth = 3;
    [SerializeField] private float moveSpeed = 5f;
    private int _currentHealth;
    private bool _isDead = false;
    
    // НОВЫЙ ПАРАМЕТР ДЛЯ СКОРОСТИ
    public float moveSpeedMultiplier = 1.0f; 

    [Header("Combat Settings")]
    public GameObject projectilePrefab; // СДЕЛАЙ ПУБЛИЧНЫМ!
    [SerializeField] private Transform firePoint;
    public int minDamage = 5;
    public int maxDamage = 7;
    [Range(0, 100)] public float critChance = 10f;
    public int pierceCount = 0; 
    public int projectilesPerShot = 1;
    public float damageMultiplier = 1.0f;
    public float spreadAngle = 10f;
    public float projectileSpeed = 10f;
    public float burstDelay = 0.1f; // Задержка между пулями при мультишоте
    
    [Header("Temporary Buff Settings")]
    public float buffDamageBonus = 0.5f;  // +50% к урону во время баффа
    public float buffSpeedBonus = 0.3f;   // +30% к скорости
    public float buffCritBonus = 25f;     // +25% к шансу крита
    
    private float _buffTimer = 0f;
    private float _buffDuration = 1f;     // Для расчета процентов в UI

    [Header("Buff UI")]
    public GameObject buffIconObject;     // Вся иконка (чтобы включать/выключать)
    public UnityEngine.UI.Image buffTimerImage; // Сама картинка (для эффекта часиков)
    

    [Header("Navigation")]
    public Node currentNode;
    private bool _isMoving = false;
    
    [Header("Special Skills")]
    public bool hasCritSplit = false;
    public bool hasDashNova = false;
    public bool hasTurretSkill = false;
    public float dashNovaCooldown = 5f; // Кулдаун способности 5 секунд (можешь менять)
    private float _dashNovaTimer = 0f;

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
    }

    void Update()
    {
        // 1. Логика таймера баффа и UI
        if (_buffTimer > 0)
        {
            _buffTimer -= Time.deltaTime;
            if (buffTimerImage != null)
            {
                buffTimerImage.fillAmount = _buffTimer / _buffDuration;
            }
            
            if (_buffTimer <= 0 && buffIconObject != null)
            {
                buffIconObject.SetActive(false); // Выключаем иконку, когда время вышло
            }
        }

        // 2. Логика кулдауна Dash Nova
        if (_dashNovaTimer > 0) _dashNovaTimer -= Time.deltaTime;

        // 3. Логика движения
        if (_isMoving && currentNode != null)
        {
            // СЧИТАЕМ СКОРОСТЬ С УЧЕТОМ БАФФА
            bool isBuffActive = _buffTimer > 0;
            float currentSpeedMult = moveSpeedMultiplier + (isBuffActive ? buffSpeedBonus : 0f);
            float currentMoveSpeed = moveSpeed * currentSpeedMult;

            transform.position = Vector3.MoveTowards(transform.position, currentNode.transform.position, currentMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentNode.transform.position) < 0.01f)
            {
                transform.position = currentNode.transform.position;
                _isMoving = false;
                UpdateAvailableNodes();

                // ВОТ ЭТА СТРОЧКА ПОТЕРЯЛАСЬ! Возвращаем её:
                if (currentNode != null) currentNode.OnPlayerArrived();

                if (hasDashNova && _dashNovaTimer <= 0f)
                {
                    FireDashNova();
                    _dashNovaTimer = dashNovaCooldown; 
                }
            }
        }
    }

    public void FireAt(Enemy target)
    {
        if (target == null || _isDead) return;
        
        // Передаем врага и его начальную позицию
        StartCoroutine(FireBurstRoutine(target, target.transform.position));
    }

    private IEnumerator FireBurstRoutine(Enemy target, Vector3 initialTargetPos)
    {
        Vector3 currentTargetPos = initialTargetPos;

        for (int i = 0; i < projectilesPerShot; i++)
        {
            if (target != null)
            {
                currentTargetPos = target.transform.position;
            }

            GameObject projGo = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile proj = projGo.GetComponent<Projectile>();

            if (proj != null)
            {
                bool isBuffActive = _buffTimer > 0;

                // УРОН С УЧЕТОМ БАФФА
                int baseDamage = Random.Range(minDamage, maxDamage + 1);
                float currentDmgMult = damageMultiplier + (isBuffActive ? buffDamageBonus : 0f);
                int scaledDamage = Mathf.RoundToInt(baseDamage * currentDmgMult);
                
                // КРИТ С УЧЕТОМ БАФФА
                float currentCrit = critChance + (isBuffActive ? buffCritBonus : 0f);
                bool isCrit = Random.value * 100f < currentCrit;
                
                int finalDamage = isCrit ? scaledDamage * 2 : scaledDamage;

                proj.speed = projectileSpeed;
                proj.pierceCount = pierceCount;
                proj.damage = finalDamage;
                proj.isCrit = isCrit;

                float randomAngleOffset = 0f;
                if (spreadAngle > 0f) randomAngleOffset = Random.Range(-spreadAngle, spreadAngle);

                proj.Launch(currentTargetPos, randomAngleOffset);
            }

            if (i < projectilesPerShot - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }
    
    private void FireDashNova()
    {
        int projectilesCount = 10;
        float angleStep = 360f / projectilesCount;

        for (int i = 0; i < projectilesCount; i++)
        {
            GameObject projGo = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile proj = projGo.GetComponent<Projectile>();

            if (proj != null)
            {
                // Берем текущие статы игрока
                int baseDamage = Random.Range(minDamage, maxDamage + 1);
                int scaledDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
                bool isCrit = Random.value * 100f < critChance;
                int finalDamage = isCrit ? scaledDamage * 2 : scaledDamage;

                proj.speed = projectileSpeed;
                proj.pierceCount = pierceCount;
                proj.damage = finalDamage;
                proj.isCrit = isCrit;

                // Высчитываем направление по кругу
                float angle = i * angleStep;
                Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                
                // Передаем координаты цели (чуть впереди в нужную сторону)
                Vector3 targetPos = transform.position + dir;
                proj.Launch(targetPos, 0f); // Разброс 0, так как они и так летят веером
            }
        }
    }

    // НОВЫЙ МЕТОД ДЛЯ СКИЛЛА НА ЖИЗНИ
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        _currentHealth += amount; // Игрок лечится при увеличении максимума
    }

    public void MoveToNode(Node targetNode)
    {
        if (_isMoving || _isDead) return;
        
        // Сообщаем старой ноде, что мы ушли
        if (currentNode != null) currentNode.OnPlayerLeft(); 

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
        
        // Выключаем фоновую музыку
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();

        if (UIManager.Instance != null && ProgressionManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(ProgressionManager.Instance.totalScore);
        }
    }
    
    // Для Аптечки
    public void Heal(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > maxHealth) _currentHealth = maxHealth;
        // Здесь позже добавим обновление UI Хелсбара игрока
    }

    // Для Бонуса (Пока просто заглушка, таймер и логику множителей добавим на следующем шаге!)
    public void ActivateTemporaryBuff(float duration)
    {
        _buffTimer = duration;
        _buffDuration = duration;
        
        if (buffIconObject != null)
        {
            buffIconObject.SetActive(true);
        }
        
        // Опционально: можно добавить визуальный эффект (Particle System) свечения на самом игроке
        Debug.Log($"Бонус активирован! Урон, скорость и крит временно повышены на {duration} сек.");
    }

    public int GetCurrentHealth() => _currentHealth;
    public bool IsDead() => _isDead;
}