using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Range(0f, 1f)]
    [SerializeField] private float nudgeMultiplier = 0.1f;

    [SerializeField] private float maxDisplacement = 2.0f;

    [SerializeField] private float followSpeed = 5.0f;

    private Vector3 basePosition;

    // Set the base position to the position of the camera at the start of the game for reference later.
    private void Awake()
    {
        basePosition = transform.position;
    }

    // After the player moved, update the camera to follow the player slightly
    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 playerDisplacement = player.position;
        playerDisplacement.y = 0;

        Vector3 offset = Vector3.ClampMagnitude(playerDisplacement * nudgeMultiplier, maxDisplacement);
        Vector3 targetPosition = basePosition + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
