using UnityEngine;

/// <summary>
/// Placed inside the main camera object.
/// Follows the player around the plot with a smooth linear interpolation and locked to a maximum offset so the player is not always centered.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Range(0f, 1f)]
    [SerializeField] private float nudgeMultiplier = 0.1f;

    [SerializeField] private float maxDisplacement = 2.0f;

    [SerializeField] private float followSpeed = 5.0f;

    private Vector3 _basePosition;

    private void Awake()
    {
        _basePosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // There is no jump feature in the game, this makes sure the player only moves in the XZ plane.
        Vector3 playerDisplacement = player.position;
        playerDisplacement.y = 0;

        // Calculates how much the camera should move given how much the player moved from the start position
        Vector3 offset = Vector3.ClampMagnitude(playerDisplacement * nudgeMultiplier, maxDisplacement);
        Vector3 targetPosition = _basePosition + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
