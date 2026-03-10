using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Settings")]
    public AudioSource bgmSource;
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    [Range(0f, 1f)] public float duckVolume = 0.15f; // Громкость во время прокачки
    public float fadeSpeed = 2f; // Скорость затихания/возврата громкости

    private Coroutine _fadeRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (bgmSource != null)
        {
            bgmSource.loop = true; // Зацикливаем трек
            bgmSource.volume = normalVolume;
            bgmSource.Play();
        }
    }

    // Метод для приглушения и возврата громкости
    public void SetUpgradeMusic(bool isUpgrading)
    {
        if (bgmSource == null) return;
        
        float targetVolume = isUpgrading ? duckVolume : normalVolume;
        
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeVolumeRoutine(targetVolume));
    }

    // Полная остановка (для экрана смерти)
    public void StopMusic()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    private IEnumerator FadeVolumeRoutine(float targetVolume)
    {
        // Используем unscaledDeltaTime, чтобы громкость менялась даже при Time.timeScale = 0
        while (Mathf.Abs(bgmSource.volume - targetVolume) > 0.01f)
        {
            bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, targetVolume, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        bgmSource.volume = targetVolume;
    }
}