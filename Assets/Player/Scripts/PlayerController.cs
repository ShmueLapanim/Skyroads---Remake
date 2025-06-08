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
    private BoxCollider _collider;
    private Coroutine _jumpBufferCoroutine;
    
    private float _currentGravity;
    private float _springForceModifier;
    
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
        float targetX = controllerSettings.horizontalSpeed * _input.HorizontalMovementInput;
        
        bool isBraking = Mathf.Sign(_rb.linearVelocity.x) != Mathf.Sign(targetX);
        isBraking = isBraking && targetX != 0 && _rb.linearVelocity.x != 0; //because 0 is considered positive we need to tak that into account
        
        float acceleration = isBraking ? 1.75f * controllerSettings.horizontalAcceleration : controllerSettings.horizontalAcceleration;
        
        Vector3 targetVelocity = _rb.linearVelocity;
        targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, targetX, acceleration * Time.fixedDeltaTime);
        
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
        if (!IsGrounded(out RaycastHit hit) || !_wantsToJump) return;
        
        _isJumping = true;
        _wantsToJump = false;
        _alignToGround = false;
        
        //compensate if we are not in the desired height
        float yCorrection = controllerSettings.groundHeight - (transform.position.y - hit.point.y);
        
        // reset the gravity so the jump height will be correct
        _currentGravity = controllerSettings.gravity;
        
        Vector3 jumpVel = _rb.linearVelocity;
        jumpVel.y = Mathf.Sqrt((controllerSettings.jumpHeight + yCorrection) * -2f * Physics.gravity.y * controllerSettings.gravity);
        _rb.linearVelocity = jumpVel;
    }

    void HandleVariableJump()
    {
        //is jumping is true when we started a jump al the way to where we land meaning that when we fall after the jump isJumping is true
        // isFalling is only true when we fall after a jump
        
        float targetGravity = CalculateTargetGravity(_rb.linearVelocity.y, _input.JumpHeld);
        _currentGravity = Mathf.MoveTowards(_currentGravity, targetGravity, controllerSettings.gravityChangeSpeed * Time.fixedDeltaTime);
        
        TryCutJump(_rb.linearVelocity.y, _input.JumpReleased);
        
        /*float targetGravity;
        if (!_isJumping)
        {
            targetGravity = controllerSettings.gravity;
        }
        else
        {
            if (!_input.JumpHeld)
            {
                // Released jump early → fall faster
                targetGravity = controllerSettings.fallGravity;
            }
            else if (Mathf.Abs(_rb.linearVelocity.y) < controllerSettings.apexThreshold)
            {
                // Near apex → reduced gravity
                targetGravity = controllerSettings.apexGravity;
            }
            else if (_rb.linearVelocity.y < 0)
            {
                // Falling after apex
                targetGravity = controllerSettings.fallGravity;
                _isFalling = true;
            }
            else
            {
                //Ascending with jump held
                targetGravity = controllerSettings.gravity;
            }
        }
        
        // Smooth gravity transition
        _currentGravity = Mathf.MoveTowards(_currentGravity, targetGravity, controllerSettings.gravityChangeSpeed * Time.fixedDeltaTime);

        // Early jump release dampening (low jump)
        if (!_isFalling && _input.JumpReleased && _rb.linearVelocity.y > controllerSettings.apexThreshold)
        {
            Vector3 jumpVel = _rb.linearVelocity;
            jumpVel.y *= controllerSettings.jumpCutMultiplier;
            _rb.linearVelocity = jumpVel;
            _isFalling = true;
            print("slowed down");
        }
        
        print(_currentGravity);
        */
        
    }
    
    private float CalculateTargetGravity(float velocityY, bool jumpHeld)
    {
        if (!_isJumping)
            return controllerSettings.gravity;

        if (!jumpHeld)
            return controllerSettings.fallGravity;

        if (Mathf.Abs(velocityY) < controllerSettings.apexThreshold)
        {
            _isFalling = true;
            return controllerSettings.apexGravity;
        }

        if (velocityY < 0f)
        {
            _isFalling = true;
            return controllerSettings.fallGravity;
        }

        return controllerSettings.gravity;
    }
    
    private void TryCutJump(float velocityY, bool jumpReleased)
    {
        if (!_isFalling && jumpReleased && velocityY > controllerSettings.apexThreshold)
        {
            Vector3 jumpVel = _rb.linearVelocity;
            jumpVel.y *= controllerSettings.jumpCutMultiplier;
            _rb.linearVelocity = jumpVel;
            _isFalling = true;
            print("slowed down");
        }
    }
    
    void ApplyGravity()
    {
        if(_alignToGround) return;
        
        Vector3 gravity = _currentGravity * Physics.gravity;
        
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
        if (!_alignToGround && IsGrounded() && _rb.linearVelocity.y <= 0f)
        {
            _alignToGround = true;
            _isJumping = false;
            _isFalling = false;
        }
        _isGrounded = IsGrounded(); // testing purposes
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

        bool isGrounded = IsGrounded(out RaycastHit hit);

        //if we are too close to the ground we want a stronger spring so we wont collide
        float distanceToGround = _collider.bounds.min.y - hit.point.y;
        _springForceModifier = distanceToGround <= 0.2f ? 10f / controllerSettings.groundSpringStrength : 1f;
        
        // spring logic when on the ground
        if (isGrounded)
        {
            float currentY = transform.position.y;
            float targetY = hit.point.y + controllerSettings.groundHeight;
            float displacement = targetY - currentY;

            // Apply spring force
            float springForce = displacement * controllerSettings.groundSpringStrength * _springForceModifier - _rb.linearVelocity.y * controllerSettings.groundSpringDamping;

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
