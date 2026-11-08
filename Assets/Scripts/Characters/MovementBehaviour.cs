using UnityEngine;
/// <summary>
/// Handles the player movement based on a public variable set in PlayerCharacter.cs
/// If the player is in a Menu the player movement gets set to 0, 0, 0.
/// </summary>
public class MovementBehaviour : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 15.0f;

    private Rigidbody _rigidbody;

    private Vector3 _desiredMovementDirection = Vector3.zero;

    public Vector3 DesiredMovementDirection
    {
        get => _desiredMovementDirection;
        set => _desiredMovementDirection = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_rigidbody == null)
        {
            return;
        }

        if (!GameManager.Instance.GameStarted)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // The movement direction is normalized for proper scaling with movement speed.
        Vector3 movement = _desiredMovementDirection.normalized;
        movement *= movementSpeed;

        if (movement.sqrMagnitude >= 1f && TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.MoveAround);
        }
        
        _rigidbody.linearVelocity = movement;
    }
}