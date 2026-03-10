using UnityEngine;

public class ChestObject : NodeInteractable
{
    [Header("Chest Settings")]
    public int xpReward = 500;

    protected override void ApplyReward()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.AddXP(xpReward);
        }
        Despawn();
    }
}