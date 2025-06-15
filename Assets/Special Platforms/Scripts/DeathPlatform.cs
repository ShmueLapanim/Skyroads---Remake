using UnityEngine;

public class DeathPlatform : MonoBehaviour, IPlatformEffect
{
    [Range(0f, 5f)] public float restartDelay = 0.5f;
    public void Apply(PlayerController player, Rigidbody unused, PlatformDetection unused1, ref Coroutine unused2)
    {
        if (!player.TryGetComponent(out PlayerDeath death)) return;
        death.Die(restartDelay);
    }

    public void Remove(PlayerController player, PlatformDetection runner, ref Coroutine coroutine)
    {
        // nothing here
    }
}
