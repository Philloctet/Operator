using UnityEngine;

public class MedkitObject : NodeInteractable
{
    [Header("Medkit Settings")]
    public int healAmount = 1;

    protected override void ApplyReward()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.Heal(healAmount);
        }
        Despawn();
    }
}