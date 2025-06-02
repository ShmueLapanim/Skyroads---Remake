using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Rigidbody _rb;
    
    [Header("Horizontal Movement Settings")]
    [SerializeField] float _horizontalSpeed;
    [SerializeField] float _horizontalAcceleration;
    
    [Header("Vertical Movement Settings")]
    [SerializeField] float _jumpHeight;
    [SerializeField] float _jumpBufferTime;
    
    [Header("Forward Movement Settings")]
    [SerializeField] float _forwardSpeed;
    
    [Header("Ground Alignment Settings")]
    [SerializeField] float _heightFromGround;
    [SerializeField] float _distanceSmothing;
    [SerializeField] float _rotationSmoothing;
    
    private bool _canJump;
    private bool _alignToGround = true;

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
        ForwardMovement();
    }

    #region Horizontal Movement
    void HorizontalMovement()
    {
        float moveDirection = _playerInput.HorizontalMovementInput;
        float targetHorizontalSpeed = _horizontalSpeed * moveDirection;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetHorizontalSpeed, _horizontalAcceleration * Time.deltaTime);
        
        _rb.linearVelocity = targetVelocity;
    }

    #endregion
    
    #region Jumping
    
    void JumpBehavior()
    {
        if (!CanJump()) return;
        
        _canJump = false;
        _alignToGround = false;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.y = Mathf.Sqrt(_jumpHeight * 2.1f * -Physics.gravity.y);
        _rb.linearVelocity += targetVelocity;
    }

    bool CanJump() //grounded + jump buffer
    {
        return IsGrounded() && _canJump;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f + _heightFromGround);
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
    
    #endregion
    
    #region Forward Movement

    void ForwardMovement()
    {
        if (!_alignToGround) return;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            
            targetVelocity.y = (_heightFromGround - distanceToGround) * _distanceSmothing;
        }

        targetVelocity.z = _forwardSpeed;
        _rb.linearVelocity = targetVelocity;
    }
    
    #endregion
}
