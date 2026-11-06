using UnityEngine;

public class BasicCharacter : MonoBehaviour
{
    protected MovementBehaviour MovementBehaviour;

    private void Awake()
    {
        MovementBehaviour = GetComponent<MovementBehaviour>();
    }
}
