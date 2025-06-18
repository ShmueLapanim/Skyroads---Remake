using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ControllerSettings", menuName = "Player/Controller Settings")]
public class PlayerControllerSettings : ScriptableObject
{
    [Header("Horizontal Movement Settings")]
    [Range(0f,20f)]public float horizontalSpeed;
    [Range(0f,200f)]public float horizontalAcceleration;
    [Range(0f,500f)]public float horizontalAccelerationChangeSpeed;
    [Range(1.1f, 2f)] public float breakingModifer;
    [Range(0f, 1f)] public float terminalHorizontalSpeedTH;
    public bool autoBrake = true;
    
    [Header("Forward Movement Settings")]
    [Range(0f,30f)]public float forwardSpeed;
    [Range(0f,100f)]public float forwardAcceleration;
    [Range(0f,100f)]public float forwardDeceleration;
    
    [Header("Gravity Settings")]
    public LayerMask groundLayer;
    [Range(0f,10f)]public float gravity;
    [Range(0f,50f)]public float terminalVelocity;
    [Range(0f, 1f)] public float terminalGravityTH;
    
    [Header("Jump Settings")]
    [Range(0f,10f)]public float jumpHeight;
    [Range(0f,1f)]public float jumpBufferTime;

    [Header("Variable Jump Settings")] 
    [Range(0f,15f)]public float fallGravity;
    [Range(0f,8f)]public float apexGravity;
    [Range(0f,10f)]public float apexThreshold;
    [Range(0f,20f)]public float gravityChangeSpeed;
    [Range(0f,1f)] public float jumpCutMultiplier;

    [Header("Ground Check Settings")] 
    public Vector3 centerOffset;
    public Vector3 halfExtents;
    
    [Header("Ground Alignment Settings")]
    [Range(0.5f, 10f)] public float groundHeight;
    [Range(0f,30f)]public float groundSpringStrength;
    [Range(0f,1f)] public float groundSpringDamping;
    
    [Header("Rotation Settings")]
    [Range(0f,20f)]public float rotationSpeed;
    [Range(0f,89f)]public float turningAngle;
    [Range(0f, 1f)] public float airRotationBlend;
}
