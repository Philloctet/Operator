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

    public void Launch(Transform target)
    {
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            _direction = new Vector2(dir.x, dir.y).normalized;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            _isInitialized = true;
        }
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
            // Передаем точное значение урона и флаг крита
            enemy.TakeDamage(damage, isCrit);

            if (pierceCount <= 0) Destroy(gameObject);
            else pierceCount--;
        }
    }
}