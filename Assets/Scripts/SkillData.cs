using UnityEngine;

public enum SkillRarity { Common, Rare, Epic }
public enum SkillCategory { Damage, MoveSpeed, CritChance, Pierce, MultiShot, MaxHP, CritSplit, DashNova, Turret }

[CreateAssetMenu(fileName = "NewSkill", menuName = "TypingShooter/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea(2, 3)]
    public string description;
    
    public SkillRarity rarity;
    public SkillCategory category;
    
    [Tooltip("Удалять ли скилл из пула после взятия?")]
    public bool isOneTime;
    
    [Tooltip("Числовое значение (например 0.05 для 5% или 1 для +1 пули)")]
    public float effectValue;
}