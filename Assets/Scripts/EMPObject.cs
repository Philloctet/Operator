using UnityEngine;

public class EMPObject : NodeInteractable
{
    [Header("EMP Settings")]
    public float freezeDuration = 10f;

    protected override void ApplyReward()
    {
        // Ищем всех врагов на сцене и замораживаем
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            enemy.Freeze(freezeDuration);
        }
        Despawn();
    }
}