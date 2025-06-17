using UnityEditor;
using UnityEngine;

public interface IPlatformEffect
{
    void Apply(PlayerController player, Rigidbody rb, ref PlatformType platformType, PlatformDetection runner, ref Coroutine coroutine);
    void Remove(PlayerController player, ref PlatformType platformType, PlatformDetection runner, ref Coroutine coroutine);
}