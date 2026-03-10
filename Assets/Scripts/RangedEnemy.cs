using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Ranged Settings")]
    public float attackRange = 7f;
    public float fireCooldown = 3f;
    public float telegraphDuration = 2f;
    public float spinSpeed = 720f; 
    
    public GameObject enemyProjectilePrefab;
    public LineRenderer aimLaser;
    
    [Header("Visuals")]
    public Transform visualSprite; // НОВОЕ: Сюда перетащим дочерний объект со спрайтом

    private float _currentCooldown = 0f;
    private bool _isPreparingToShoot = false;

    protected override void Start()
    {
        // ВАЖНО: Вызываем базовый Start, чтобы загрузить слово и настроить слайдер ХП!
        base.Start(); 

        if (aimLaser != null)
        {
            aimLaser.enabled = false;
            aimLaser.useWorldSpace = true; 
        }
    }

    protected override void FixedUpdate()
    {
        if (_isDying || PlayerController.Instance == null || PlayerController.Instance.IsDead()) return;

        // Если ЭМИ-бомба заморозила
        if (_freezeTimer > 0)
        {
            _freezeTimer -= Time.fixedDeltaTime;
            return; 
        }

        // Если мы готовимся к выстрелу - стоим на месте и крутимся
        if (_isPreparingToShoot) return;

        // Обновляем кулдаун выстрела
        if (_currentCooldown > 0) _currentCooldown -= Time.fixedDeltaTime;

        Vector2 playerPos = PlayerController.Instance.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        // Логика движения: идем к игроку, пока не достигнем attackRange
        if (distanceToPlayer > attackRange)
        {
            Vector2 newPos = Vector2.MoveTowards(transform.position, playerPos, moveSpeed * Time.fixedDeltaTime);
            if (_rb != null) _rb.MovePosition(newPos);
            else transform.position = newPos;
        }
        else
        {
            // Мы на дистанции атаки! Если кулдаун прошел - начинаем стрелять
            if (_currentCooldown <= 0)
            {
                StartCoroutine(ShootRoutine());
            }
        }
    }

    private IEnumerator ShootRoutine()
    {
        _isPreparingToShoot = true;

        // 1. Фиксируем позицию игрока (туда полетит пуля и туда светит лазер)
        Vector3 lockedTargetPos = PlayerController.Instance.transform.position;

        // 2. Включаем лазер
        if (aimLaser != null)
        {
            aimLaser.enabled = true;
            aimLaser.SetPosition(0, transform.position);
            aimLaser.SetPosition(1, lockedTargetPos);
        }

        // 3. Крутимся вокруг своей оси (Telegraphing)
        float timer = 0f;
        while (timer < telegraphDuration)
        {
            timer += Time.deltaTime;
            
            if (_isDying || _freezeTimer > 0) 
            {
                if (aimLaser != null) aimLaser.enabled = false;
                _isPreparingToShoot = false;
                
                // Возвращаем спрайт в нормальное положение при прерывании
                if (visualSprite != null) visualSprite.localRotation = Quaternion.identity;
                
                yield break;
            }

            // КРУТИМ ТОЛЬКО СПРАЙТ
            if (visualSprite != null)
            {
                visualSprite.Rotate(0, 0, spinSpeed * Time.deltaTime);
            }
            
            if (aimLaser != null) aimLaser.SetPosition(0, transform.position);
            
            yield return null;
        }

        // 4. Выстрел!
        if (aimLaser != null) aimLaser.enabled = false;

        GameObject projGo = Instantiate(enemyProjectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile proj = projGo.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Launch(lockedTargetPos);
        }

        // 5. Уходим в кулдаун
        _currentCooldown = fireCooldown;
        _isPreparingToShoot = false;
        
        // ВОЗВРАЩАЕМ СПРАЙТ В ИСХОДНОЕ ПОЛОЖЕНИЕ
        if (visualSprite != null) visualSprite.localRotation = Quaternion.identity;
    }
}