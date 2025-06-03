using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Rigidbody _rb;
    
    [Header("Horizontal Movement Settings")]
    [SerializeField] float _horizontalSpeed;
    [SerializeField] float _horizontalAcceleration;
    [SerializeField] float _turningAngle;
    [SerializeField] float _turningSpeed;
    
    [Header("Vertical Movement Settings")]
    [SerializeField] float _gravityMultiplier;
    [SerializeField] float _jumpHeight;
    [SerializeField] float _jumpBufferTime;
    
    [Header("Forward Movement Settings")]
    [SerializeField] float _forwardSpeed;
    [SerializeField] float _forwardAcceleration;
    
    [Header("Ground Alignment Settings")]
    [SerializeField] float _heightFromGround;
    [SerializeField] float _distanceSpringStrength;
    [SerializeField] float _distanceSpringDamping;
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
        ForwardMovement();
        GroundAlignment2();
        HandleRotation();
        JumpBehavior();
        ApplyGravity();
    }

    private void Update()
    {
        JumpBuffer();
        CheckGroundAlignment();
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
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt(_jumpHeight * 2.1f * -Physics.gravity.y * _gravityMultiplier);
        _rb.linearVelocity = jumpVel;
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
    
    #region Gravity Handling

    void ApplyGravity()
    {
        if(_alignToGround) return;
        
        Vector3 gravity = Vector3.zero;
        gravity.y = _gravityMultiplier * Physics.gravity.y;
        
        _rb.linearVelocity += gravity * Time.fixedDeltaTime;
    }
    
    #endregion
    
    #region Forward Movement

    void GroundAlignment()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, _heightFromGround + 1f) && _alignToGround)
        {
            // align on the y
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            targetVelocity.y = (_heightFromGround - distanceToGround) * _distanceSpringStrength;

            //smoothing the alignment on the y
            targetVelocity.x = _rb.linearVelocity.x;
            targetVelocity.z = _rb.linearVelocity.z;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, _distanceSpringDamping * Time.fixedDeltaTime);
        }
    }
    
    void ForwardMovement()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.z = _forwardSpeed;
        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, _forwardAcceleration * Time.fixedDeltaTime);
    }
    void GroundAlignment2()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, _heightFromGround + 1f) && _alignToGround)
        {
            float currentY = transform.position.y;
            float targetY = hit.point.y + _heightFromGround;
            float displacement = targetY - currentY;

            // Apply spring force
            float springForce = displacement * _distanceSpringStrength - _rb.linearVelocity.y * _distanceSpringDamping;

            Vector3 targetVelocity = _rb.linearVelocity;
            targetVelocity.y += springForce;

            _rb.linearVelocity = targetVelocity;
        }
        else
        {
            _alignToGround = false;
        }
    }


    

    void CheckGroundAlignment()
    {
        if(_alignToGround) return;
        
        if(_rb.linearVelocity.y > 0) return;

        if (Physics.Raycast(transform.position, Vector3.down, _heightFromGround + 1f))
        {
            _alignToGround = true;
        }
    }
    
    #endregion
    
    void HandleRotation()
    {
        Quaternion baseRotation;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _heightFromGround * 2f) && _alignToGround)
        {
            // Align rotation to slope normal
            baseRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            // Face movement direction in air
            Vector3 lookDirection = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
            baseRotation = Quaternion.LookRotation(lookDirection.normalized + transform.forward * 2f);
        }

        // Extract baseRotation's Euler angles
        Vector3 euler = baseRotation.eulerAngles;

        // Apply Z-axis tilt based on player input
        float targetZ = -_playerInput.HorizontalMovementInput * _turningAngle;
        euler.z = targetZ;
        euler.y = 0f;

        // Apply final rotation
        Quaternion finalRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, _rotationSmoothing * Time.fixedDeltaTime);
    }

}
