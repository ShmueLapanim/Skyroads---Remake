using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ControllerSettings", menuName = "Player/Controller Settings")]
public class PlayerControllerSettings : ScriptableObject
{
    [Header("Horizontal Movement Settings")]
    public float horizontalSpeed;
    public float horizontalAcceleration;
    
    [Header("Forward Movement Settings")]
    public float forwardSpeed;
    public float forwardAcceleration;
    
    [Header("Gravity Settings")]
    public float gravity;
    public float terminalVelocity;
    
    [Header("Jump Settings")]
    public float jumpHeight;
    public float jumpBufferTime;

    [Header("Variable Jump Settings")] 
    public float fallGravity;
    public float apexGravity;
    public float apexThreshold;
    public float gravityChangeSpeed;
    [Range(0f,1f)] public float jumpCutMultiplier;

    [Header("Ground Check Settings")] 
    public Vector3 centerOffset;
    public Vector3 halfExtents;
    
    [Header("Ground Alignment Settings")]
    [Range(0.5f, 10f)] public float groundHeight;
    public float groundSpringStrength;
    [Range(0f,1f)] public float groundSpringDamping;
    
    [Header("Rotation Settings")]
    public float rotationSpeed;
    public float turningAngle;
    [Range(0f, 1f)] public float airRotationBlend;
}
