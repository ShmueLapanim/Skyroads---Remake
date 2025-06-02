using System;
using System.Collections;
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
    [SerializeField] float _jumpBufferTime;
    
    private bool _canJump;

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

    private void Update()
    {
        JumpBuffer();
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
        if (!CanJump()) return;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.y = Mathf.Sqrt(_jumpHeight * 2.1f * -Physics.gravity.y);
        _rb.linearVelocity += targetVelocity;
    }

    bool CanJump() //grounded + jump buffer
    {
        return Physics.Raycast(transform.position, -Vector3.up, 0.6f) && _canJump;
    }

    void JumpBuffer()
    {
        if (!_playerInput.JumpPressed || _canJump) return;
        
        _canJump = true;
            
        if(_jumpBufferCoroutine != null)
            StopCoroutine(_jumpBufferCoroutine);
            
        _jumpBufferCoroutine = StartCoroutine(JumpBufferCoroutine());
    }

    
    Coroutine _jumpBufferCoroutine;
    IEnumerator JumpBufferCoroutine()
    {
        yield return new WaitForSeconds(_jumpBufferTime);
        _canJump = false;
    }
}
