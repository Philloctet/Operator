using UnityEngine;
using TMPro;

public class UpgradeCardUI : MonoBehaviour, ITypable
{
    public TMP_Text titleText;
    public TMP_Text wordDisplay;
    
    [Header("Colors")]
    public Color highlightColor = Color.green;

    private string _word;
    private int _upgradeType; // 0: Multishot, 1: Pierce, 2: Damage

    public void SetupRandomUpgrade()
    {
        _upgradeType = Random.Range(0, 3);
        
        titleText.text = _upgradeType switch {
            0 => "+1 Projectile",
            1 => "+1 Pierce",
            2 => "+20% Total Damage",
            _ => "Generic Upgrade"
        };

        // Получаем слово. Убедитесь, что WordProvider инициализирован!
        _word = WordProvider.Instance.GetUniqueWord(WordType.Upgrade);
        if (wordDisplay != null) wordDisplay.text = _word;
        
        TypingManager.Instance.RegisterTypable(this);
    }

    public string GetWord() => _word;

    public void OnCharTyped(int index)
    {
        if (string.IsNullOrEmpty(_word) || wordDisplay == null) return;

        string typed = _word.Substring(0, index);
        string remaining = _word.Substring(index);
        wordDisplay.text = $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{typed}</color>{remaining}";
    }

    public void OnReset()
    {
        if (wordDisplay != null) wordDisplay.text = _word;
    }

    public void OnComplete()
    {
        ApplyEffect();
        
        if (WordProvider.Instance != null)
            WordProvider.Instance.ReleaseWord(_word);
            
        TypingManager.Instance.UnregisterTypable(this);
        
        // Завершаем процесс улучшения через менеджер прогрессии
        ProgressionManager.Instance.CompleteUpgrade();
    }

    private void ApplyEffect()
    {
        var pc = PlayerController.Instance;
        if (pc == null) return;

        switch (_upgradeType)
        {
            case 0: 
                pc.projectilesPerShot++; 
                break;
            case 1: 
                pc.pierceCount++; 
                break;
            case 2: 
                pc.damageMultiplier += 0.2f; 
                break;
        }
    }

    public Transform GetTransform() => transform;
}