using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;

    [Header("Abilities")]
    public int pierceCount = 0; // Сколько врагов пуля может пробить насквозь

    private Transform _target;
    private Vector2 _direction;
    private bool _isInitialized = false;

    /// <summary>
    /// Инициализация полета снаряда в сторону врага
    /// </summary>
    public void Launch(Transform target)
    {
        if (target != null)
        {
            // ПРАВИЛЬНАЯ ФОРМУЛА: Куда (цель) МИНУС Откуда (пуля)
            Vector3 dir = target.position - transform.position;
            _direction = new Vector2(dir.x, dir.y).normalized;

            // Поворачиваем сам спрайт пули в сторону полета
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            _isInitialized = true;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!_isInitialized) return;

        // Используем движение в мировом пространстве через direction
        transform.position += (Vector3)_direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.TakeHit();
            if (pierceCount <= 0) Destroy(gameObject);
            else pierceCount--;
        }
    }
}