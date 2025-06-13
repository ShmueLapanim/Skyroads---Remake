using UnityEngine;

public class SlipperyPlatform : MonoBehaviour, IPlatformEffect
{
    [Range(0f, 20f)] public float slipperyAcceleration = 10f;
    [Range(0f, 20f)] public float slipperySpeed = 10f;
    public void Apply(PlayerController player, Rigidbody rigidbody)
    {
        player.RuntimeSettings.horizontalAcceleration = slipperyAcceleration;
        player.RuntimeSettings.horizontalSpeed = slipperySpeed;
        player.RuntimeSettings.breakingModifer = 1f;

        player.RuntimeSettings.autoBrake = false;
    }

    public void Remove(PlayerController player)
    {
        player.RuntimeSettings.horizontalAcceleration = player.DefaultSettings.horizontalAcceleration;
        player.RuntimeSettings.horizontalSpeed = player.DefaultSettings.horizontalSpeed;
        player.RuntimeSettings.breakingModifer = player.DefaultSettings.breakingModifer;
        
        player.RuntimeSettings.autoBrake = player.DefaultSettings.autoBrake;
    }
}
