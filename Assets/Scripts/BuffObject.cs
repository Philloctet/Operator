using UnityEngine;

public class BuffObject : NodeInteractable
{
    [Header("Buff Settings")]
    public float duration = 15f;

    protected override void ApplyReward()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ActivateTemporaryBuff(duration);
        }
        Despawn();
    }
}