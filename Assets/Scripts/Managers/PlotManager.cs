using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    private Camera _mainCamera;
    private Plot _currentlyHoveredPlot;

    public List<Plot> ActivePlots { get; private set; } = new List<Plot>();

    private void Start()
    {
        _mainCamera = Camera.main;
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
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHoveredPlot();
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Plot hitPlot = hit.collider.GetComponentInParent<Plot>();
            if (hitPlot != null)
            {
                if (!IsPlayerInRange(hit.transform))
                {
                    ClearHoveredPlot();
                    return;
                }
                if (hitPlot != _currentlyHoveredPlot)
                {
                    ClearHoveredPlot();
                    _currentlyHoveredPlot = hitPlot;
                    _currentlyHoveredPlot.OnHoverEnter();
                }

                return;
            }
        }
        
        ClearHoveredPlot();
    }
    
    private void HandleClick()
    {
        if (_currentlyHoveredPlot == null)
        {
            return;
        }
        if (!IsPlayerInRange(_currentlyHoveredPlot.transform))
        {
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentlyHoveredPlot != null)
        {
            _currentlyHoveredPlot.Interact();
        }
    }

    private void ClearHoveredPlot()
    {
        if (_currentlyHoveredPlot != null)
        {
            _currentlyHoveredPlot.OnHoverExit();
            _currentlyHoveredPlot = null;
        }
    }

    private void CreatePlots()
    {
        if (plotTemplate == null)
        {
            return;
        }
        for (int index = 0; index < plotRows * plotColumns; ++index)
        {
            GameObject clone = Instantiate(plotTemplate, transform);
            clone.name = "Plot_" + index.ToString();

            int row = index / plotColumns;
            int column = index % plotColumns;

            clone.transform.position = new Vector3(startPosition.x + (row * spacing.x), startPosition.y, startPosition.z +
                (column * spacing.z));
            
            ActivePlots.Add(clone.GetComponent<Plot>());
        }
    }
}
