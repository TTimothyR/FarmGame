using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Handles player movement input and sends the values to MovementBehaviour.cs to handle the actual movement.
/// Only reads movement input while the player is not in a Menu.
/// </summary>
public class PlayerCharacter : BasicCharacter
{
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private InputActionReference xMovement;
    [SerializeField] private InputActionReference yMovement;

    private void OnEnable()
    {
        if (inputAsset == null)
        {
            return;
        }
        
        inputAsset.Enable();
    }

    private void Update()
    {
        if (GameManager.Instance.GameStarted)
        {
            HandleMovementInput();
        }
    }

    private void HandleMovementInput()
    {
        if (MovementBehaviour == null || xMovement == null || yMovement == null)
        {
            return;
        }

        float horizontalMovementInput = xMovement.action.ReadValue<float>();
        float verticalMovementInput = yMovement.action.ReadValue<float>();

        Vector3 movement = (horizontalMovementInput * Vector3.forward) + (verticalMovementInput * Vector3.left);
        MovementBehaviour.DesiredMovementDirection = movement;
    }
}
