using UnityEngine;

public class DeathPlatform : MonoBehaviour, IPlatformEffect
{
    public void Apply(PlayerController player, Rigidbody unused, ref PlatformType platformType, PlatformDetection unused1, ref Coroutine unused2)
    {
        platformType = PlatformType.Death;
        if (!player.TryGetComponent(out PlayerDeath death)) return;
        death.Die();
    }

    public void Remove(PlayerController player, ref PlatformType platformType, PlatformDetection runner, ref Coroutine coroutine)
    {
        // nothing here
    }
}
