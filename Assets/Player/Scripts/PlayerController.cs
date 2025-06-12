using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerControllerSettings defaultSettings;
    private PlayerControllerSettings _currentSettings;
    
    private PlayerInput _input;
    private Rigidbody _rb;
    private BoxCollider _collider;
    private Coroutine _jumpBufferCoroutine;
    
    private float _currentGravity;
    private float _springForceModifier;
    private float _currentHorizontalAcceleration;
    
    private bool _wantsToJump;
    private bool _alignToGround = true;
    private bool _isJumping = false;
    private bool _isFalling = false;
    private bool _isGrounded;// for testing
    

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponentInChildren<BoxCollider>();
        _currentSettings = defaultSettings;
        
        if(_collider == null)
            Debug.LogError("No child box collider attached");
    }
    
    private void Update()
    {
        HandleJumpInput();
        CheckGroundStatus();
        HandleVariableJump();
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        ApplyForwardMovement();
        ApplyRotation();
        GroundAlignment2();
        ApplyGravity();
        HandleJump();
    }

    

    #region Movement Logic
    void HandleHorizontalMovement()
    {
        float targetVelX = defaultSettings.horizontalSpeed * _input.HorizontalMovementInput;
        
        //isBraking simply means: are we trying to go the opposite way we are going right now?
        bool isBraking = !Mathf.Approximately(Mathf.Sign(_rb.linearVelocity.x), Mathf.Sign(targetVelX));
        isBraking = isBraking && targetVelX != 0 && _rb.linearVelocity.x != 0; //because 0 is considered positive we need to take that into account
        
        float targetAcceleration = isBraking ? 1.75f * defaultSettings.horizontalAcceleration : defaultSettings.horizontalAcceleration;
        
        bool isReachingMaxSpeed = Mathf.Abs(_rb.linearVelocity.x) > Mathf.Abs(targetVelX) * defaultSettings.terminalHorizontalSpeedTH;
        isReachingMaxSpeed = isReachingMaxSpeed && targetVelX != 0 && Mathf.Approximately(Mathf.Sign(_rb.linearVelocity.x), Mathf.Sign(targetVelX));

        float maxSpeedAcceleration =
                Helper.MapValue(Mathf.Abs(_rb.linearVelocity.x),
                Mathf.Abs(targetVelX) * defaultSettings.terminalHorizontalSpeedTH, Mathf.Abs(targetVelX),
                targetAcceleration, 0.1f);
        
        _currentHorizontalAcceleration = isReachingMaxSpeed ? maxSpeedAcceleration : isBraking || targetVelX == 0f ? targetAcceleration :
            Mathf.MoveTowards(_currentHorizontalAcceleration, targetAcceleration, defaultSettings.horizontalAccelerationChangeSpeed * Time.fixedDeltaTime);
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetVelX, _currentHorizontalAcceleration * Time.fixedDeltaTime);
        
        print($"{_currentHorizontalAcceleration} , {_rb.linearVelocity.x}");
        _rb.linearVelocity = targetVelocity;
    }
    
    void ApplyForwardMovement()
    {
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.z = defaultSettings.forwardSpeed;
        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, defaultSettings.forwardAcceleration * Time.fixedDeltaTime);
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
        yield return new WaitForSeconds(defaultSettings.jumpBufferTime);
        _wantsToJump = false;
    }

    void HandleJump()
    {
        if (!IsGrounded(out RaycastHit hit) || !_wantsToJump) return;
        
        _isJumping = true;
        _wantsToJump = false;
        _alignToGround = false;
        
        //compensate if we are not in the desired height (the ground spring)
        float yCorrection = defaultSettings.groundHeight - (transform.position.y - hit.point.y);
        
        // reset the gravity so the jump height will be correct
        _currentGravity = defaultSettings.gravity;
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt((defaultSettings.jumpHeight + yCorrection) * -2f * Physics.gravity.y * defaultSettings.gravity);
        _rb.linearVelocity = jumpVel;
    }

    void HandleVariableJump()
    {
        // is jumping is true when we started a jump al the way to where we land meaning that when we fall after the jump isJumping is true
        // isFalling is only true when we fall after a jump
        
        // if we are at the terminal gravity threshold we dont want to change the gravity
        // the CapFallSpeed() has different cehavior
        if(_rb.linearVelocity.y < -defaultSettings.terminalVelocity * defaultSettings.terminalGravityTH) return;
        
        float targetGravity = CalculateTargetGravity(_rb.linearVelocity.y, _input.JumpHeld);
        _currentGravity = Mathf.MoveTowards(_currentGravity, targetGravity, defaultSettings.gravityChangeSpeed * Time.fixedDeltaTime);
        
        TryCutJump(_rb.linearVelocity.y, _input.JumpReleased);
    }
    
    private float CalculateTargetGravity(float velocityY, bool jumpHeld)
    {
        if (!_isJumping)
            return defaultSettings.gravity;

        if (!jumpHeld)
            return defaultSettings.fallGravity;

        if (Mathf.Abs(velocityY) < defaultSettings.apexThreshold)
        {
            _isFalling = true;
            return defaultSettings.apexGravity;
        }

        if (velocityY < 0f)
        {
            _isFalling = true;
            return defaultSettings.fallGravity;
        }

        return defaultSettings.gravity;
    }
    
    private void TryCutJump(float velocityY, bool jumpReleased)
    {
        if (!_isFalling && jumpReleased && velocityY > defaultSettings.apexThreshold)
        {
            Vector3 jumpVel = _rb.linearVelocity;
            jumpVel.y *= defaultSettings.jumpCutMultiplier;
            _rb.linearVelocity = jumpVel;
            _isFalling = true;
        }
    }
    
    void ApplyGravity()
    {
        if(_alignToGround) return;
        
        Vector3 gravity = _currentGravity * Physics.gravity;
        
        _rb.linearVelocity += gravity * Time.fixedDeltaTime;
        
        CapFallSpeed();
    }

    void CapFallSpeed()
    { 
        //cap the fall speed
        Vector3 cappedVelocity = _rb.linearVelocity;
        cappedVelocity.y = Mathf.Clamp(cappedVelocity.y, -defaultSettings.terminalVelocity, Mathf.Infinity);
        _rb.linearVelocity = cappedVelocity;
        
        //smooth transition of the gravity towards reaching terminal velocity
        float gravity = _isJumping ? defaultSettings.fallGravity : defaultSettings.gravity;
        
        _currentGravity = _rb.linearVelocity.y < -defaultSettings.terminalVelocity * defaultSettings.terminalGravityTH ? 
                            Helper.MapValue(_rb.linearVelocity.y, 
                            -defaultSettings.terminalVelocity, -defaultSettings.terminalVelocity * defaultSettings.terminalGravityTH
                            , 0.1f, gravity) : 
                            _currentGravity;
    }

    
    
    #endregion
    
    #region Ground Checks and Alignment

    private void CheckGroundStatus()
    {
        if (!_alignToGround && IsGrounded() && _rb.linearVelocity.y <= 0f)
        {
            _alignToGround = true;
            _isJumping = false;
            _isFalling = false;
        }
        _isGrounded = IsGrounded(); // testing purposes
    }
    
    void GroundAlignment2()
    {
        if(!_alignToGround) return;

        bool isGrounded = IsGrounded(out RaycastHit hit);

        //if we are too close to the ground we want a stronger spring so we wont collide
        float distanceToGround = _collider.bounds.min.y - hit.point.y;
        _springForceModifier = distanceToGround <= 0.2f ? 10f / defaultSettings.groundSpringStrength : 1f;
        
        // spring logic when on the ground
        if (isGrounded)
        {
            float currentY = transform.position.y;
            float targetY = hit.point.y + defaultSettings.groundHeight;
            float displacement = targetY - currentY;

            // Apply spring force
            float springForce = displacement * defaultSettings.groundSpringStrength * _springForceModifier - _rb.linearVelocity.y * defaultSettings.groundSpringDamping;

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
            transform.position + defaultSettings.centerOffset, 
            defaultSettings.halfExtents, 
            Vector3.down, 
            out hit,Quaternion.identity,
            defaultSettings.groundHeight + extraDistance, 
            defaultSettings.groundLayer
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

        if (_alignToGround && IsGrounded(out RaycastHit hit))
        {
            // Align rotation to slope normal
            alignRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            // Face movement direction in air
            Vector3 lookDirection = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
            Vector3 blendDirection = Vector3.Lerp(Vector3.forward, lookDirection, defaultSettings.airRotationBlend);
            alignRotation = Quaternion.LookRotation(blendDirection);
        }

        // Extract baseRotation's Euler angles
        Vector3 euler = alignRotation.eulerAngles;

        // Apply Z-axis tilt based on player input
        float inputRotation = -_input.HorizontalMovementInput * defaultSettings.turningAngle;
        euler.z = inputRotation;
        euler.y = 0f;

        // Apply final rotation
        Quaternion finalRotation = Quaternion.Euler(euler);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, defaultSettings.rotationSpeed * Time.fixedDeltaTime);
    }

    #endregion
    

    #if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        float castDistance = defaultSettings.groundHeight;
        Vector3 boxHalfExtents = defaultSettings.halfExtents; // Change to match your actual BoxCast size
        Vector3 castDirection = Vector3.down;

        // Calculate the center of the box at the end of the cast
        Vector3 start = transform.position + defaultSettings.centerOffset;
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
        
        // Draw the spring box
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(end, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        // Draw the line between start and end
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(start, end);
    }
    
    #endif
}
