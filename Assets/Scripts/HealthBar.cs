using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void UpdateHealth(int current, int max)
    {
        // Включаем хелсбар только при получении урона
        if (!gameObject.activeSelf) 
        {
            gameObject.SetActive(true);
        }
        
        slider.maxValue = max;
        slider.value = current;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}