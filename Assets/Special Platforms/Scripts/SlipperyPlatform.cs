using UnityEngine;

public class SlipperyPlatform : MonoBehaviour, IPlatformEffect
{
    [Range(0f, 20f)] [Tooltip("overrides horizontal acceleration by this number")] 
    public float slipperyAcceleration = 10f;
    [Range(0f, 20f)] [Tooltip("overrides horizontal speed by this number")]
    public float slipperySpeed = 10f;
    public void Apply(PlayerController player, Rigidbody rigidbody, PlatformDetection runner, ref Coroutine coroutine)
    {
        player.RuntimeSettings.horizontalAcceleration = slipperyAcceleration;
        player.RuntimeSettings.horizontalSpeed = slipperySpeed;
        player.RuntimeSettings.breakingModifer = 1f;

        player.RuntimeSettings.autoBrake = false;
    }

    public void Remove(PlayerController player, PlatformDetection runner, ref Coroutine coroutine)
    {
        player.RuntimeSettings.horizontalAcceleration = player.DefaultSettings.horizontalAcceleration;
        player.RuntimeSettings.horizontalSpeed = player.DefaultSettings.horizontalSpeed;
        player.RuntimeSettings.breakingModifer = player.DefaultSettings.breakingModifer;
        
        player.RuntimeSettings.autoBrake = player.DefaultSettings.autoBrake;
    }
}
