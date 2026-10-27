using System;
using UnityEngine;


public class Plot : MonoBehaviour
{
    [SerializeField] private ItemData wateringCan;
    [Header("Colors")] 
    [SerializeField] private Color baseColor = new Color(144/255f, 83/255f, 45/255f);
    [SerializeField] private Color wateredColor;

    [Header("Crop growing")]
    [SerializeField] private float growTime = 10f;
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 3f;
    
    [Header("Crop rotting")]
    [SerializeField] private float rotTime = 20f;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.saddleBrown;
    
    public bool IsOccupied => currentCrop != null;
    private bool isWatered = false;
    private bool isHovering = false;
    private GameObject currentCrop = null;
    private ItemData currentCropData = null;
    private float currentCropElapsedTime = 0f;
    private float elapsedWateredTime = 0f;
    private float maximumWateredTime = 0f;
    private Renderer currentCropRenderer;
    private Renderer plotRenderer;

    private void Awake()
    {
        if (plotRenderer == null)
        {
            plotRenderer = GetComponentInChildren<Renderer>();
        }
        ResetColor();
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }
        if (isWatered)
        {
            DryOutOverTime();
        }
        
        // Grow the crop while in its growing stage and while it's in the correct season.
        if (SeasonManager.Instance.GetSeasonalCrop() == currentCropData)
        {
            if (IsOccupied && Math.Floor(currentCropElapsedTime/growTime) <= 0 && currentCrop != null && isWatered)
            {
                GrowCrop();
            }
        }
        // Rot the crop when it left the growing stage
        if (IsOccupied && Math.Floor(currentCropElapsedTime / growTime) > 0 && currentCropRenderer != null)
        {
            RotCrop();
        }

        if (IsOccupied && currentCropElapsedTime > (growTime + rotTime) && currentCrop != null)
        {
            DestroyCrop();
        }
    }

    private float EaseInSine(float progress)
    {
        return 1 - Mathf.Cos((progress * Mathf.PI) / 2f);
    }

    private void DryOutOverTime()
    {
        elapsedWateredTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedWateredTime / maximumWateredTime);
        float easedProgress = EaseInSine(progress);

        Color currentColor = Color.Lerp(wateredColor, baseColor, easedProgress);
        plotRenderer.material.color = currentColor;
        if (elapsedWateredTime >= maximumWateredTime)
        {
            ResetWatering();
        }
    }

    private void GrowCrop()
    {
        currentCropElapsedTime += Time.deltaTime;
        float t = currentCropElapsedTime / growTime;
        float scale = Mathf.Lerp(startScale, endScale, t);
        currentCrop.transform.localScale = Vector3.one * scale;
    }

    private void RotCrop()
    {
        currentCropElapsedTime += Time.deltaTime;
        float t = (currentCropElapsedTime - growTime) / rotTime;
        currentCropRenderer.material.color = Color.Lerp(startColor, endColor, t);
    }

    private void ResetWatering()
    {
        isWatered = false;
        elapsedWateredTime = 0f;
        maximumWateredTime = 0f;
        SetColor();
    }

    private void RemoveCurrentCrop()
    {
        Destroy(currentCrop);
        currentCropRenderer = null;
        currentCropElapsedTime = 0f;
        currentCropData = null;
        ResetWatering();
    }

    private void DestroyCrop()
    {
        RemoveCurrentCrop();
        GameManager.Instance.LoseLife();
    }

    private void HarvestCrop()
    {
        RemoveCurrentCrop();
        GameManager.Instance.GainScore();
    }

    public void OnHoverEnter()
    {
        isHovering = true;
        SetColor();
    }

    public void OnHoverExit()
    {
        ResetColor();
    }
    
    

    public void Interact()
    {
        // Plant a crop if the player is holding a seed and the plot is not yet occupied
        if (Inventory.Instance.HoldingItem == wateringCan && !isWatered)
        {
            isWatered = true;
            maximumWateredTime = SeasonManager.Instance.GetDryOutTime();
            SetColor();
        } 
        else if (Inventory.Instance.HoldingItem != wateringCan && Inventory.Instance.HoldingItem != null && !IsOccupied)
        {
            GameObject cropClone = Instantiate(Inventory.Instance.HoldingItem.prefab, transform);
            currentCrop = cropClone;
            currentCropData = Inventory.Instance.HoldingItem;
            currentCropRenderer = cropClone.GetComponent<Renderer>();
        }
        else if (IsOccupied && currentCropElapsedTime >= growTime && currentCropElapsedTime < (growTime + rotTime))
        {
            HarvestCrop();
        }
    }

    private void SetColor()
    {
        if (plotRenderer != null)
        {
            Color targetColor = isWatered ? wateredColor : baseColor;
            if (isHovering)
            {
                plotRenderer.material.color = Color.Lerp(targetColor, Color.white, 0.2f);
            }
            else
            {
                plotRenderer.material.color = targetColor;
            }
        }
    }

    private void ResetColor()
    {
        isHovering = false;
        SetColor();
    }
}
