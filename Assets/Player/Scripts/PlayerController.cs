using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
   
    
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float horizontalAcceleration;
    
    
    [Header("Jump Settings")]
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpBufferTime;
    
    [Header("Forward Movement Settings")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float forwardAcceleration;
    
    [Header("Ground Alignment Settings")]
    [SerializeField] private float groundHeight;
    [SerializeField] private float groundSpringStrength;
    [SerializeField, Range(0f,1f)] private float groundSpringDamping;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float turningAngle;
    [SerializeField, Range(0f, 1f)] private float airRotationBlend;
    
    private PlayerInput _input;
    private Rigidbody _rb;
    private Coroutine _jumpBufferCoroutine;
    
    private bool _wantsToJump;
    private bool _alignToGround = true;
    private bool _isGrounded;

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        HandleJumpInput();
        CheckGroundStatus();
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        ApplyForwardMovement();
        GroundAlignment2();
        ApplyRotation();
        HandleJump();
        ApplyGravity();
    }

    

    #region Movement Logic
    void HandleHorizontalMovement()
    {
        float targetX = horizontalSpeed * _input.HorizontalMovementInput;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetX, horizontalAcceleration * Time.fixedDeltaTime);
        
        _rb.linearVelocity = targetVelocity;
    }
    
    void ApplyForwardMovement()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.z = forwardSpeed;
        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, forwardAcceleration * Time.fixedDeltaTime);
    }

    #endregion
    
    #region Jumping and Gravity
    
    void HandleJumpInput()
    {
        if (!_input.JumpPressed) return;
        
        _wantsToJump = true;
            
        if(_jumpBufferCoroutine != null)
            StopCoroutine(_jumpBufferCoroutine);
            
        _jumpBufferCoroutine = StartCoroutine(JumpBufferCoroutine());
    }
    
    IEnumerator JumpBufferCoroutine()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        _wantsToJump = false;
    }

    void HandleJump()
    {
        if (!_isGrounded || !_wantsToJump) return;
        
        _wantsToJump = false;
        _alignToGround = false;
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt(jumpHeight * -2.1f * Physics.gravity.y * gravityMultiplier);
        _rb.linearVelocity = jumpVel;
    }
    
    void ApplyGravity()
    {
        if(_alignToGround) return;
        
        Vector3 gravity = gravityMultiplier * Physics.gravity;
        
        _rb.linearVelocity += gravity * Time.fixedDeltaTime;
    }

    
    
    #endregion
    
    #region Ground Checks and Alignment

    private void CheckGroundStatus()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, groundHeight + 1);
        
        if (!_alignToGround && _isGrounded && _rb.linearVelocity.y <= 0f)
        {
            _alignToGround = true;
        }
    }
    void GroundAlignment()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundHeight + 1f) && _alignToGround)
        {
            // align on the y
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            targetVelocity.y = (groundHeight - distanceToGround) * groundSpringStrength;

            //smoothing the alignment on the y
            targetVelocity.x = _rb.linearVelocity.x;
            targetVelocity.z = _rb.linearVelocity.z;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, groundSpringDamping * Time.fixedDeltaTime);
        }
    } // ignore
    
    void GroundAlignment2()
    {
        if(!_alignToGround) return;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundHeight + 1f))
        {
            float currentY = transform.position.y;
            float targetY = hit.point.y + groundHeight;
            float displacement = targetY - currentY;

            // Apply spring force
            float springForce = displacement * groundSpringStrength - _rb.linearVelocity.y * groundSpringDamping;

            Vector3 targetVelocity = _rb.linearVelocity;
            targetVelocity.y += springForce;

            _rb.linearVelocity = targetVelocity;
        }
        else
        {
            _alignToGround = false;
        }
    }

    
    
    #endregion

    #region Rotation

    void ApplyRotation()
    {
        Quaternion alignRotation;

        if (_alignToGround && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundHeight + 1f))
        {
            // Align rotation to slope normal
            alignRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            // Face movement direction in air
            Vector3 lookDirection = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
            Vector3 blendDirection = Vector3.Lerp(Vector3.forward, lookDirection, airRotationBlend);
            alignRotation = Quaternion.LookRotation(blendDirection);
        }

        // Extract baseRotation's Euler angles
        Vector3 euler = alignRotation.eulerAngles;

        // Apply Z-axis tilt based on player input
        float inputRotation = -_input.HorizontalMovementInput * turningAngle;
        euler.z = inputRotation;
        euler.y = 0f;

        // Apply final rotation
        Quaternion finalRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    #endregion
    

}
