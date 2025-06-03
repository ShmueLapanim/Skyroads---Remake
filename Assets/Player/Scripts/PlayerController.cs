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
    
    [Header("Ground Alignment Settings")]
    [SerializeField] float _heightFromGround;
    [SerializeField] float _distanceSmothing;
    [SerializeField] float _distanceDamping;
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
        GroundAlignment();
        TurningRotation();
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

    void TurningRotation()
    {
        float targetRotationZ = -_playerInput.HorizontalMovementInput * _turningAngle;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.rotation.x, transform.rotation.y, targetRotationZ), _turningSpeed * Time.fixedDeltaTime);
    }

    #endregion
    
    #region Jumping
    
    void JumpBehavior()
    {
        if (!CanJump()) return;
        
        _canJump = false;
        _alignToGround = false;
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt(_jumpHeight * 4f * -Physics.gravity.y * _gravityMultiplier);
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
        
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _heightFromGround * 2f) && _alignToGround)
        {
            // align on the y
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            targetVelocity.y = (_heightFromGround - distanceToGround) * _distanceSmothing;
            
            // align the rotation
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            slopeRotation.z = transform.rotation.z;
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation, _rotationSmoothing * Time.fixedDeltaTime);
            
            //smoothing the alignment on the y
            targetVelocity.x = _rb.linearVelocity.x;
            targetVelocity.z = _rb.linearVelocity.z;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, _distanceDamping * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 lookDirection = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
            Quaternion velRotation = Quaternion.LookRotation(lookDirection.normalized + transform.forward * 2f);
            transform.rotation = Quaternion.Slerp(transform.rotation, velRotation, _rotationSmoothing * Time.fixedDeltaTime);
        }
        
        
    }

    void ForwardMovement()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.z = _forwardSpeed;
        _rb.linearVelocity = targetVelocity;
    }

    void CheckGroundAlignment()
    {
        if(_alignToGround) return;
        
        if(_rb.linearVelocity.y > 0) return;

        if (Physics.Raycast(transform.position, Vector3.down, _heightFromGround))
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

        // Apply final rotation
        Quaternion finalRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, _rotationSmoothing * Time.fixedDeltaTime);
    }

}
