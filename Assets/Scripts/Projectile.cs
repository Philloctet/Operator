using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;
    public float lifeTime = 5f;
    
    [HideInInspector] public int damage;    // Устанавливается игроком при выстреле
    [HideInInspector] public bool isCrit;  // Флаг критического удара
    public int pierceCount = 0;

    private Vector2 _direction;
    private bool _isInitialized = false;

    public void Launch(Vector3 targetPos, float angleOffset = 0f)
    {
        // 1. Находим базовое направление к цели
        Vector3 dir = targetPos - transform.position;
        
        // 2. Высчитываем базовый угол в градусах
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // 3. Прибавляем наш разброс (spreadAngle)
        float finalAngle = baseAngle + angleOffset;

        // 4. Переводим итоговый угол обратно в вектор направления для полета
        _direction = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;

        // 5. Поворачиваем спрайт пули
        transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
        
        _isInitialized = true;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!_isInitialized) return;
        transform.position += (Vector3)_direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.TakeDamage(damage, isCrit);

            // НАВЫК: Разлет при крите
            if (isCrit && PlayerController.Instance.hasCritSplit)
            {
                SplitProjectile();
            }

            if (pierceCount <= 0) Destroy(gameObject);
            else pierceCount--;
        }
    }

    private void SplitProjectile()
    {
        // Узнаем текущий угол полета пули в градусах
        float currentAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

        // Создаем две новые пули под углами 45 и -45
        CreateSplitProjectile(currentAngle + 45f);
        CreateSplitProjectile(currentAngle - 45f);
    }

    private void CreateSplitProjectile(float angle)
    {
        // Берем префаб прямо из контроллера игрока
        GameObject projPrefab = PlayerController.Instance.projectilePrefab;
        GameObject projGo = Instantiate(projPrefab, transform.position, Quaternion.identity);
        Projectile proj = projGo.GetComponent<Projectile>();

        if (proj != null)
        {
            // Наследуем характеристики родительской пули
            proj.speed = speed;
            proj.pierceCount = pierceCount;
            proj.damage = damage;
            proj.isCrit = isCrit; // Оставляем крит, чтобы они МОГЛИ разделяться бесконечно!

            // Высчитываем новый вектор направления
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            
            // Смещаем позицию старта немного по вектору, чтобы пуля не ударила того же врага снова
            proj.transform.position = transform.position + (dir * 0.5f);

            // Запускаем
            Vector3 targetPos = proj.transform.position + dir;
            proj.Launch(targetPos, 0f);
        }
    }
}