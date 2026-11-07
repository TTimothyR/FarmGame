using UnityEngine;

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

        Vector3 movement = _desiredMovementDirection.normalized;
        movement *= movementSpeed;

        if (movement.sqrMagnitude >= 1f && TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.MoveAround);
        }
        
        movement.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = movement;
    }
}