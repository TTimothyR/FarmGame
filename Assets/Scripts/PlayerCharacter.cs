using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : BasicCharacter
{
    [SerializeField] private InputActionAsset _inputAsset;
    [SerializeField] private InputActionReference _xMovement;
    [SerializeField] private InputActionReference _yMovement;
    

    protected override void Awake()
    {
        base.Awake();

        if (_inputAsset == null)
        {
            return;
        }
    }

    private void OnEnable()
    {
        if (_inputAsset == null)
        {
            return;
        }
        
        _inputAsset.Enable();
    }

    private void Update()
    {
        if (GameManager.Instance.GameStarted)
        {
            HandleMovementInput();
        }
    }

    // Handle the input for both movement directions
    private void HandleMovementInput()
    {
        if (_movementBehaviour == null || _xMovement == null || _yMovement == null)
        {
            return;
        }

        float horizontalMovementInput = _xMovement.action.ReadValue<float>();
        float verticalMovementInput = _yMovement.action.ReadValue<float>();

        Vector3 movement = (horizontalMovementInput * Vector3.forward) + (verticalMovementInput * Vector3.left);
        _movementBehaviour.DesiredMovementDirection = movement;
    }
}
