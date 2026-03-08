using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TMP_Text _text;
    private float _moveSpeed = 2f;
    private float _disappearTimer = 0.8f;
    private Color _textColor;

    public void Setup(int damageAmount, bool isCrit)
    {
        // Ищем текст и на самом объекте, и во всех вложенных объектах
        _text = GetComponentInChildren<TMP_Text>();
        
        // Защита от краша: если текст всё-таки не найден, ругаемся в консоль и прерываем метод
        if (_text == null)
        {
            Debug.LogError("DamagePopup: Не найден компонент TMP_Text в префабе!");
            return;
        }

        _text.text = damageAmount.ToString();
        
        if (isCrit)
        {
            _text.color = Color.yellow;
            _text.fontSize *= 1.5f;
            _moveSpeed = 3f; // Криты отлетают быстрее
        }
        else
        {
            _text.color = Color.white;
        }
        
        _textColor = _text.color;
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        // Если текста нет, двигать и прозрачить нечего
        if (_text == null) return;

        transform.position += Vector3.up * _moveSpeed * Time.deltaTime;
        
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0.4f)
        {
            float alpha = Mathf.Clamp01(_disappearTimer / 0.4f);
            _text.color = new Color(_textColor.r, _textColor.g, _textColor.b, alpha);
        }
    }
}