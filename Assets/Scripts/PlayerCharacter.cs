using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : BasicCharacter
{
    [SerializeField] private InputActionAsset _inputAsset;
    [SerializeField] private InputActionReference _xMovement;
    [SerializeField] private InputActionReference _yMovement;
    
    private InteractBehaviour _interactBehaviour;

    private InputAction _interactAction;

    protected override void Awake()
    {
        base.Awake();
        _interactBehaviour = GetComponent<InteractBehaviour>();

        if (_inputAsset == null)
        {
            return;
        }

        _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");
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
        HandleMovementInput();
        HandleInteractInput();
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

    private void HandleInteractInput()
    {
        if (_interactBehaviour == null || _interactAction == null)
        {
            return;
        }

        if (_interactAction.WasPressedThisFrame())
        {
            _interactBehaviour.Interact();
        }
    }
}
