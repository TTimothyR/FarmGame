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

    private void Awake()
    {
        basePosition = transform.position;
    }

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
