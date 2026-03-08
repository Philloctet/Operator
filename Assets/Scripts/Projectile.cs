using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _speed;
    private int _damage;
    private int _pierceCount;
    private Vector3 _direction;

    public void Setup(Vector3 direction, float speed, int damage, int pierce)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        _pierceCount = pierce;

        // Поворот снаряда по направлению полета
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Самоуничтожение через 5 секунд, если снаряд улетел за экран
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, попали ли мы во врага
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.TakeDamage(_damage);

            if (_pierceCount <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                _pierceCount--;
            }
        }
    }
}