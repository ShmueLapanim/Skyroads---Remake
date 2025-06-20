using System;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    
    public float bulletSpeed;
    public float bulletLifeTime;
    
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }
    private void Update()
    {
        ShootBehavior();
    }

    void ShootBehavior()
    {
        if (!_playerInput.ShootPressed) return;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
        
        if (!bullet.TryGetComponent(out Bullet bulletScript)) return;
        bulletScript.SetVariables(bulletSpeed, bulletLifeTime);
    }
}
