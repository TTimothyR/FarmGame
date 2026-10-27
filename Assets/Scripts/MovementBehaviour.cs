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
    
    // Handles collision with the ground.
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

    // Update the player position based on direction and movement speed.
    private void HandleMovement()
    {
        if (_rigidbody == null)
        {
            return;
        }

        Vector3 movement = _desiredMovementDirection.normalized;
        movement *= _movementSpeed;
        movement.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = movement;
    }
}
