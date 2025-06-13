using UnityEditor;
using UnityEngine;

public interface IPlatformEffect
{
    void Apply(PlayerController player, Rigidbody rb);
    void Remove(PlayerController player);
}