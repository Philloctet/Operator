using UnityEngine;
using System.Collections;

public class Turret : NodeInteractable
{
    [Header("Turret Settings")]
    public float activeDuration = 10f;
    public float fireRate = 0.5f;
    public float attackRange = 10f;

    // Переопределяем абстрактный метод из NodeInteractable.
    // Он автоматически вызовется базовым классом, когда игрок допечатает слово.
    protected override void ApplyReward()
    {
        StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        float timer = activeDuration;

        while (timer > 0)
        {
            ShootNearestEnemy();
            yield return new WaitForSeconds(fireRate);
            timer -= fireRate;
        }

        // Время вышло - сообщаем старому менеджеру начать отсчет
        if (TurretManager.Instance != null)
        {
            TurretManager.Instance.StartCooldown(); 
        }
        
        // Вызываем универсальный метод удаления из базового класса
        Despawn(); 
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