using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class InsectManager : MonoBehaviour
{
    public static InsectManager Instance { get; private set; }
    
    [SerializeField] private PlotManager plotManager;
    [SerializeField] private int tutorialPlotIndex = 11;
    [SerializeField] private GameObject goodInsectTemplate;
    [SerializeField] private GameObject badInsectTemplate;
    [SerializeField] private GameObject spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float badInsectSpawnRate = 0.5f;
    
    [Header("Range checking")]
    [SerializeField] private GameObject player;
    [SerializeField] private float maximumKillDistance = 4f;

    private float _timer;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }

        if (TutorialManager.Instance.IsTutorialActive)
        {
            HandleClick();
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnRandomInsect();
        }

        if (Mouse.current == null)
        {
            return;
        }
        HandleClick();
    }
    
    private bool IsPlayerInRange(Transform target)
    {
        float distance = (player.transform.position - target.position).magnitude;

        return distance <= maximumKillDistance;
    }

    private void HandleClick()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && Inventory.Instance.HoldingItem == Inventory.Instance.Shovel)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Insect hitInsect = hit.collider.GetComponentInParent<Insect>();
                if (hitInsect != null)
                {
                    if (!IsPlayerInRange(hit.transform))
                    {
                        return;
                    }
                    
                    TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.KillInsect);
                    hitInsect.Kill(true);
                }
            }
        }
    }

    public void SpawnTutorialInsect()
    {
        int index = Random.Range(0, spawnPoints.transform.childCount);
        Transform spawnPoint = spawnPoints.transform.GetChild(index);

        Plot targetPlot = plotManager.ActivePlots[tutorialPlotIndex];
        
        GameObject insectToSpawn = badInsectTemplate;
        GameObject clone = Instantiate(insectToSpawn, transform);
        clone.transform.position = spawnPoint.position;
        clone.transform.rotation = Quaternion.identity;
        
        Insect insect = clone.GetComponent<Insect>();

        if (insect != null)
        {
            targetPlot.AddInsect(insect);
            insect.Initialize(targetPlot, true);
        }
    }

    private void SpawnRandomInsect()
    {
        int index = Random.Range(0, spawnPoints.transform.childCount);
        Transform spawnPoint = spawnPoints.transform.GetChild(index);

        Plot targetPlot = GetFurthestOccupiedPlot(spawnPoint.position);

        if (targetPlot == null)
        {
            return;
        }
        
        GameObject insectToSpawn = (Random.value < badInsectSpawnRate) ? badInsectTemplate : goodInsectTemplate;
        GameObject clone = Instantiate(insectToSpawn, transform);
        clone.transform.position = spawnPoint.position;
        clone.transform.rotation = Quaternion.identity;
        
        Insect insect = clone.GetComponent<Insect>();

        if (insect != null)
        {
            targetPlot.AddInsect(insect);
            insect.Initialize(targetPlot, false);
        }
    }

    private Plot GetFurthestOccupiedPlot(Vector3 spawnPosition)
    {
        List<Plot> allPlots = plotManager.ActivePlots;

        Plot furthestPlot = null;
        float maxDistance = -1f;

        foreach (Plot plot in allPlots)
        {
            if (!plot.IsOccupied || plot.HasInsect)
            {
                continue;
            }

            float distance = Vector3.Distance(plot.transform.position, spawnPosition);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPlot = plot;
            }
        }

        return furthestPlot;
    }
}
