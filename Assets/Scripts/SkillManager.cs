using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Skill Pool")]
    public List<SkillData> allSkills; // Сюда перетянем все созданные скиллы
    private List<SkillData> _availableSkills;

    [Header("Rarity Weights (%)")]
    [Range(0, 100)] public float commonWeight = 70f;
    [Range(0, 100)] public float rareWeight = 25f;
    [Range(0, 100)] public float epicWeight = 5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Копируем все скиллы в пул доступных
        _availableSkills = new List<SkillData>(allSkills);
    }

    public List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> chosenSkills = new List<SkillData>();
        List<SkillCategory> usedCategories = new List<SkillCategory>();

        for (int i = 0; i < count; i++)
        {
            SkillData selected = RollSkill(usedCategories);
            if (selected != null)
            {
                chosenSkills.Add(selected);
                usedCategories.Add(selected.category); // Запоминаем категорию, чтобы не выдать дубликат
            }
        }
        return chosenSkills;
    }

    private SkillData RollSkill(List<SkillCategory> excludedCategories)
    {
        // 1. Бросаем кубик на редкость
        float roll = Random.Range(0f, commonWeight + rareWeight + epicWeight);
        SkillRarity targetRarity = SkillRarity.Common;

        if (roll > commonWeight + rareWeight) targetRarity = SkillRarity.Epic;
        else if (roll > commonWeight) targetRarity = SkillRarity.Rare;

        // 2. Ищем скиллы нужной редкости и БЕЗ повторений категорий
        var candidates = _availableSkills.Where(s => s.rarity == targetRarity && !excludedCategories.Contains(s.category)).ToList();

        // 3. Защита от пустого пула: если нужной редкости нет, берем любую доступную
        if (candidates.Count == 0)
        {
            candidates = _availableSkills.Where(s => !excludedCategories.Contains(s.category)).ToList();
        }

        // 4. Выбираем случайный из подходящих
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return null; // Пул полностью пуст
    }

    public void ApplySkill(SkillData skill)
    {
        if (skill.isOneTime)
        {
            _availableSkills.Remove(skill);
        }
        
        PlayerController pc = PlayerController.Instance;
        if (pc == null) return;

        // Применяем эффект в зависимости от категории
        switch (skill.category)
        {
            case SkillCategory.Damage:
                // effectValue = 0.05 для 5%, 0.50 для 50%
                pc.damageMultiplier += skill.effectValue; 
                break;
                
            case SkillCategory.MoveSpeed:
                pc.moveSpeedMultiplier += skill.effectValue;
                break;
                
            case SkillCategory.CritChance:
                // Если базовый крит 10 (10%), а скилл дает +5%, effectValue = 5
                pc.critChance += skill.effectValue; 
                break;
                
            case SkillCategory.Pierce:
                // effectValue = 1 для +1 пробития
                pc.pierceCount += Mathf.RoundToInt(skill.effectValue);
                break;
                
            case SkillCategory.MultiShot:
                pc.projectilesPerShot += Mathf.RoundToInt(skill.effectValue);
                break;
                
            case SkillCategory.MaxHP:
                pc.IncreaseMaxHealth(Mathf.RoundToInt(skill.effectValue));
                break;
            
            case SkillCategory.CritSplit:
                pc.hasCritSplit = true;
                break;
                
            case SkillCategory.DashNova:
                pc.hasDashNova = true;
                break;

            // Эти мы реализуем на Этапах 3 и 4
            case SkillCategory.Turret:
                Debug.Log("Активирована спец. способность!");
                break;
        }

        Debug.Log($"Применен скилл: {skill.skillName} | Новое значение применено!");
    }
}