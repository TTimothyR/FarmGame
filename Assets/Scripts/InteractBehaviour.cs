using System;
using UnityEngine;

public class InteractBehaviour : MonoBehaviour
{
    public void Interact()
    {
        HUD.Instance.ShowNotification("You cannot perform this action.");
    }
}
