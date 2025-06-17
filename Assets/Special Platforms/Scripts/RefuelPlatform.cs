using UnityEngine;

public class RefuelPlatform : MonoBehaviour, IPlatformEffect
{
    [Range(0f,100f)][Tooltip("Per unit Traveled")] public float fillAmount;
    
    private bool _isRefueling;
    private PlayerFuel _playerFuel;

    void Update()
    {
        if(!_isRefueling) return;
        
        _playerFuel.Refuel(fillAmount);
    }
    public void Apply(PlayerController player, Rigidbody rb, ref PlatformType platformType, PlatformDetection runner, ref Coroutine coroutine)
    {
        if(!player.TryGetComponent(out _playerFuel)) return;
        
        platformType = PlatformType.Refuel;
        _isRefueling = true;
    }

    public void Remove(PlayerController player, ref PlatformType platformType, PlatformDetection runner, ref Coroutine coroutine)
    {
        platformType = platformType == PlatformType.Refuel ? PlatformType.None : platformType;
        _isRefueling = false;
        _playerFuel = null;
    }
}
