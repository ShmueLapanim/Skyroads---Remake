using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, PlayerControls.IMovementActions
{
    public PlayerControls PlayerControls { get; private set; }
    public float HorizontalMovementInput { get; private set; }
    public bool JumpPressed { get; private set; } // will be reset to false after we jump in PlayerMovement script
    
    private void OnEnable()
    {
        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
    }

    private void Update()
    {
        print(JumpPressed);
    }

    
    void EnableInput()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.Movement.Enable();
        PlayerControls.Movement.SetCallbacks(this);
    }
    
    void DisableInput()
    {
        PlayerControls.Movement.Disable();
        PlayerControls.Movement.RemoveCallbacks(this);
        
        PlayerControls.Dispose();
        PlayerControls = null;
    }

   

    public void OnSideMovement(InputAction.CallbackContext context)
    {
        HorizontalMovementInput = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            JumpPressed = true;
    }
    
    public void ResetJumpInput()
    {
        JumpPressed = false;
    }
}
