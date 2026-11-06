using UnityEngine;
using UnityEngine.InputSystem;

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
