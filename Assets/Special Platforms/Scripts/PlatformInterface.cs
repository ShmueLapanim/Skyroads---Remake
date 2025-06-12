using UnityEditor;
using UnityEngine;

public interface IPlatformEffect
{
    void Apply(PlayerController player);
    void Remove(PlayerController player);
}