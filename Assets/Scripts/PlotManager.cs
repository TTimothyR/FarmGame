using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlotManager : MonoBehaviour
{
    [SerializeField] private int plotRows = 4;
    [SerializeField] private int plotColumns = 4;
    [SerializeField] private GameObject plotTemplate;
    [SerializeField] private Vector3 startPosition = new Vector3(-24f, 0.6f, -20f);
    [SerializeField] private Vector3 spacing = new Vector3(10f, 0, 10f);
    
    [Header("Range checking")]
    [SerializeField] private GameObject player;
    [SerializeField] private float maximumInteractionDistance = 5f;

    private Camera mainCamera;
    private Plot currentlyHoveredPlot = null;

    private void Start()
    {
        mainCamera = Camera.main;
        CreatePlots();
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }
        if (Mouse.current == null)
        {
            return;
        }

        if (player == null)
        {
            return;
        }
        
        HandleMouseHover();
        HandleClick();
    }

    private bool IsPlayerInRange(Transform targetPlot)
    {
        float distance = (player.transform.position - targetPlot.position).magnitude;

        return distance <= maximumInteractionDistance;
    }

    private void HandleMouseHover()
    {
        // Action is ignored if it was processed by the UI.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHoveredPlot();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the object is actually a plot based on the Plot component.
            Plot hitPlot = hit.collider.GetComponentInParent<Plot>();
            if (hitPlot != null)
            {
                if (!IsPlayerInRange(hit.transform))
                {
                    ClearHoveredPlot();
                    return;
                }
                if (hitPlot != currentlyHoveredPlot)
                {
                    ClearHoveredPlot();
                    currentlyHoveredPlot = hitPlot;
                    currentlyHoveredPlot.OnHoverEnter();
                }

                return;
            }
        }
        
        ClearHoveredPlot();
    }
    
    // TODO: Range check, player can only interact with a plot while in range.
    private void HandleClick()
    {
        if (currentlyHoveredPlot == null)
        {
            return;
        }
        if (!IsPlayerInRange(currentlyHoveredPlot.transform))
        {
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyHoveredPlot != null)
        {
            currentlyHoveredPlot.Interact();
        }
    }

    // Clear the currently hovering plot and reset the color.
    private void ClearHoveredPlot()
    {
        if (currentlyHoveredPlot != null)
        {
            currentlyHoveredPlot.OnHoverExit();
            currentlyHoveredPlot = null;
        }
    }

    private void CreatePlots()
    {
        for (int index = 0; index < plotRows * plotColumns; ++index)
        {
            GameObject clone = Instantiate(plotTemplate, transform);
            clone.name = "Plot_" + index.ToString();

            int row = index / plotColumns;
            int column = index % plotColumns;

            clone.transform.position = new Vector3(startPosition.x + (row * spacing.x), startPosition.y, startPosition.z +
                (column * spacing.z));
        }
    }
}
