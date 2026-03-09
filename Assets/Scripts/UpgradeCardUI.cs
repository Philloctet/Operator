using UnityEngine;
using TMPro;

public class UpgradeCardUI : MonoBehaviour, ITypable
{
    public TMP_Text titleText;
    public TMP_Text descriptionText; // НОВОЕ: Поле для описания скилла
    public TMP_Text wordDisplay;
    
    [Header("Rarity Colors")]
    public Color commonColor = Color.white;
    public Color rareColor = new Color(0.2f, 0.6f, 1f); // Голубой
    public Color epicColor = new Color(0.8f, 0.2f, 1f); // Фиолетовый
    public Color highlightColor = Color.green;

    private string _word;
    private SkillData _currentSkill;

    public void SetupSkill(SkillData skill)
    {
        _currentSkill = skill;
        
        // Заполняем тексты
        titleText.text = skill.skillName;
        if (descriptionText != null) descriptionText.text = skill.description;

        // Красим заголовок (или фон) в цвет редкости
        titleText.color = skill.rarity switch {
            SkillRarity.Common => commonColor,
            SkillRarity.Rare => rareColor,
            SkillRarity.Epic => epicColor,
            _ => commonColor
        };

        _word = WordProvider.Instance.GetUniqueWord(WordType.Upgrade);
        if (wordDisplay != null) 
        {
            wordDisplay.text = _word;
            wordDisplay.color = Color.white;
        }
        
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
        // Передаем скилл в менеджер для применения
        SkillManager.Instance.ApplySkill(_currentSkill);
        
        if (WordProvider.Instance != null) WordProvider.Instance.ReleaseWord(_word);
        TypingManager.Instance.UnregisterTypable(this);
        ProgressionManager.Instance.CompleteUpgrade();
    }

    public Transform GetTransform() => transform;
}