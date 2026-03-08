using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float amplitude = 10f; // Высота плавания
    public float speed = 2f;      // Скорость
    private Vector3 _startPos;

    void OnEnable() // Срабатывает каждый раз, когда текст включается
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        // Используем синусоиду для движения вверх-вниз
        float newY = _startPos.y + Mathf.Sin(Time.unscaledTime * speed) * amplitude;
        transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);
    }
}