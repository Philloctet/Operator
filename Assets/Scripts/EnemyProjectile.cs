using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifetime = 5f;

    private Vector2 _direction;
    private bool _isInitialized = false;

    public void Launch(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        _direction = new Vector2(dir.x, dir.y).normalized;
        
        // Поворачиваем пулю носом по направлению полета
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        _isInitialized = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (_isInitialized)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что попали именно в игрока
        if (collision.CompareTag("Player") && collision.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}