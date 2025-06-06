using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerControllerSettings controllerSettings;
    
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
        float targetX = controllerSettings.horizontalSpeed * _input.HorizontalMovementInput;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetX, controllerSettings.horizontalAcceleration * Time.fixedDeltaTime);
        
        _rb.linearVelocity = targetVelocity;
    }
    
    void ApplyForwardMovement()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.z = controllerSettings.forwardSpeed;
        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, controllerSettings.forwardAcceleration * Time.fixedDeltaTime);
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
        yield return new WaitForSeconds(controllerSettings.jumpBufferTime);
        _wantsToJump = false;
    }

    void HandleJump()
    {
        if (!IsGrounded() || !_wantsToJump) return;
        
        _wantsToJump = false;
        _alignToGround = false;
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt(controllerSettings.jumpHeight * -2.1f * Physics.gravity.y * controllerSettings.gravityMultiplier);
        _rb.linearVelocity = jumpVel;
    }
    
    void ApplyGravity()
    {
        if(_alignToGround) return;
        print("yay");
        Vector3 gravity = controllerSettings.gravityMultiplier * Physics.gravity;
        
        _rb.linearVelocity += gravity * Time.fixedDeltaTime;
        
        //cap the fall speed
        Vector3 cappedVelocity = _rb.linearVelocity;
        cappedVelocity.y = Mathf.Clamp(cappedVelocity.y, -controllerSettings.terminalVelocity, Mathf.Infinity);
        _rb.linearVelocity = cappedVelocity;
    }

    
    
    #endregion
    
    #region Ground Checks and Alignment

    private void CheckGroundStatus()
    {
        if (!_alignToGround && IsGrounded(0.5f) && _rb.linearVelocity.y <= 0f)
        {
            _alignToGround = true;
        }
        _isGrounded = IsGrounded();
    }
    
    void GroundAlignment()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, controllerSettings.groundHeight + 1f) && _alignToGround)
        {
            // align on the y
            float distanceToGround = Vector3.Distance(transform.position, hit.point);
            targetVelocity.y = (controllerSettings.groundHeight - distanceToGround) * controllerSettings.groundSpringStrength;

            //smoothing the alignment on the y
            targetVelocity.x = _rb.linearVelocity.x;
            targetVelocity.z = _rb.linearVelocity.z;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, controllerSettings.groundSpringDamping * Time.fixedDeltaTime);
        }
    } // ignore
    
    void GroundAlignment2()
    {
        if(!_alignToGround) return;

        if (IsGrounded(out RaycastHit hit, 0.5f))
        {
            float currentY = transform.position.y;
            float targetY = hit.point.y + controllerSettings.groundHeight;
            float displacement = targetY - currentY;

            // Apply spring force
            float springForce = displacement * controllerSettings.groundSpringStrength - _rb.linearVelocity.y * controllerSettings.groundSpringDamping;

            Vector3 targetVelocity = _rb.linearVelocity;
            targetVelocity.y += springForce;

            _rb.linearVelocity = targetVelocity;
        }
        else
        {
            _alignToGround = false;
        }
    }
    
    private bool IsGrounded(out RaycastHit hit, float extraDistance = 0f)
    {
        return Physics.BoxCast
        (
            transform.position + controllerSettings.centerOffset, 
            controllerSettings.halfExtents, 
            Vector3.down, 
            out hit,Quaternion.identity,
            controllerSettings.groundHeight + extraDistance
        ); 
    }
    
    private bool IsGrounded(float extraDistance = 0f)
    {
        return IsGrounded(out _, extraDistance);
    }

    
    
    #endregion

    #region Rotation

    void ApplyRotation()
    {
        Quaternion alignRotation;

        if (_alignToGround && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controllerSettings.groundHeight + 1f))
        {
            // Align rotation to slope normal
            alignRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            // Face movement direction in air
            Vector3 lookDirection = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
            Vector3 blendDirection = Vector3.Lerp(Vector3.forward, lookDirection, controllerSettings.airRotationBlend);
            alignRotation = Quaternion.LookRotation(blendDirection);
        }

        // Extract baseRotation's Euler angles
        Vector3 euler = alignRotation.eulerAngles;

        // Apply Z-axis tilt based on player input
        float inputRotation = -_input.HorizontalMovementInput * controllerSettings.turningAngle;
        euler.z = inputRotation;
        euler.y = 0f;

        // Apply final rotation
        Quaternion finalRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, controllerSettings.rotationSpeed * Time.fixedDeltaTime);
    }

    #endregion
    

    #if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        float castDistance = controllerSettings.groundHeight;
        Vector3 boxHalfExtents = controllerSettings.halfExtents; // Change to match your actual BoxCast size
        Vector3 castDirection = Vector3.down;

        // Calculate the center of the box at the end of the cast
        Vector3 start = transform.position + controllerSettings.centerOffset;
        Vector3 end = start + castDirection * castDistance;
        Quaternion orientation = Quaternion.identity;

        // Draw the starting box (optional)
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(start, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        // Draw the ending box
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(end, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
        
        // Draw the ending box
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(end + new Vector3(0, -0.5f, 0), orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        // Draw the line between start and end
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(start, end);
    }
    
    #endif
}
