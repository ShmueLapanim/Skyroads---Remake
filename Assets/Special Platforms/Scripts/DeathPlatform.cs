using UnityEngine;

public class DeathPlatform : MonoBehaviour, IPlatformEffect
{
    public void Apply(PlayerController player, Rigidbody unused, PlatformDetection unused1, ref Coroutine unused2)
    {
        if (!player.TryGetComponent(out PlayerDeath death)) return;
        death.Die();
    }

    public void Remove(PlayerController player, PlatformDetection runner, ref Coroutine coroutine)
    {
        // nothing here
    }
}
