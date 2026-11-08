using UnityEngine;
/// <summary>
/// Base class used in PlayerCharacter to handle movement.
/// </summary>
public class BasicCharacter : MonoBehaviour
{
    protected MovementBehaviour MovementBehaviour;

    private void Awake()
    {
        MovementBehaviour = GetComponent<MovementBehaviour>();
    }
}
