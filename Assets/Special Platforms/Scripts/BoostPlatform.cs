using System.Collections;
using UnityEngine;

public class BoostPlatform : MonoBehaviour, IPlatformEffect
{
    public float initialImpulseModifier;
    public float springModifier;
    public float jumpModifier;
    public float boostSpeed = 20f;
    public float boostAcceleration = 20f;
    public float boostDuration = 0.5f;
    
    private Coroutine _boostCoroutine;
    public void Apply(PlayerController player, Rigidbody rb)
    {
        Vector3 boost = rb.linearVelocity;
        boost.z = player.RuntimeSettings.forwardSpeed * initialImpulseModifier;
        rb.linearVelocity = boost;
        
        player.RuntimeSettings.forwardAcceleration = boostAcceleration;
        player.RuntimeSettings.forwardSpeed = boostSpeed;
        player.RuntimeSettings.groundSpringStrength *= springModifier;
        player.RuntimeSettings.jumpHeight *= jumpModifier;
    }

    public void Remove(PlayerController player)
    {
        if(_boostCoroutine != null)
            StopCoroutine(_boostCoroutine);

        _boostCoroutine = StartCoroutine(StopBoost(player, boostDuration));
    }

    IEnumerator StopBoost(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        player.RuntimeSettings.forwardAcceleration = player.DefaultSettings.forwardAcceleration;
        player.RuntimeSettings.forwardSpeed = player.DefaultSettings.forwardSpeed;
        player.RuntimeSettings.groundSpringStrength = player.DefaultSettings.groundSpringStrength;
        player.RuntimeSettings.jumpHeight = player.DefaultSettings.jumpHeight;
    }
}
