using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Rigidbody _rb;
    
    [Header("Horizontal Movement")]
    [SerializeField] float _horizontalSpeed;
    [SerializeField] float _horizontalAcceleration;
    
    [Header("Vertical Movement")]
    [SerializeField] float _jumpHeight;
    
    

    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HorizontalMovement();
        JumpBehavior();
    }

    void HorizontalMovement()
    {
        float moveDirection = _playerInput.HorizontalMovementInput;
        float targetHorizontalSpeed = _horizontalSpeed * moveDirection;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetHorizontalSpeed, _horizontalAcceleration * Time.deltaTime);
        
        _rb.linearVelocity = targetVelocity;
    }

    void JumpBehavior()
    {
        if (_playerInput.JumpPressed && IsGrounded())
        {
            Vector3 targetVelocity = _rb.linearVelocity;
            targetVelocity.y = Mathf.Sqrt(_jumpHeight * 2.1f * -Physics.gravity.y);
            _rb.linearVelocity = targetVelocity;
            
            _playerInput.ResetJumpInput();
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.6f);
    }
}
