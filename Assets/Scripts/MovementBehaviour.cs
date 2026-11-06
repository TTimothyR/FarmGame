using System;
using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 15.0f;

    private Rigidbody _rigidbody;

    private Vector3 _desiredMovementDirection = Vector3.zero;

    private bool _grounded = false;

    private const float GROUND_CHECK_DISTANCE = 0.2f;
    private const string GROUND_LAYER = "Ground";

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
        
        _grounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            GROUND_CHECK_DISTANCE,
            LayerMask.GetMask(GROUND_LAYER)
        );
    }

    private void HandleMovement()
    {
        if (_rigidbody == null)
        {
            return;
        }

        if (!GameManager.Instance.GameStarted)
        {
            _rigidbody.linearVelocity = new Vector3(0, 0, 0);
            return;
        }

        Vector3 movement = _desiredMovementDirection.normalized;
        movement *= _movementSpeed;

        if (movement.sqrMagnitude >= 1f && TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.MoveAround);
        }
        
        movement.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = movement;
    }
}
