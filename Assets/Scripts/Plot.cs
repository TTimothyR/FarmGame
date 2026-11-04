using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Plot : MonoBehaviour
{
    [SerializeField] private ItemData wateringCan;
    [SerializeField] private ItemData shovel;
    [SerializeField] private Sprite waterDropIcon;
    [SerializeField] private Sprite exclamationMarkIcon;
    
    [Header("Needs Icon Animation")]
    [SerializeField] private Canvas plotNeeds;
    [SerializeField] private float smallScale = 0.01f;
    [SerializeField] private float bigScale = 0.012f;
    [SerializeField] private float pulseSpeed = 6f;
    
    [Header("Colors")] 
    [SerializeField] private Color baseColor = new Color(144/255f, 83/255f, 45/255f);
    [SerializeField] private Color wateredColor;

    [Header("Crop growing")]
    [SerializeField] private float growTime = 10f;
    [SerializeField] private float timeUntilRotStart = 3f;
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 3f;
    
    [Header("Crop rotting")]
    [SerializeField] private float rotTime = 20f;
    [SerializeField] private float endRotation = 45f;
    [SerializeField] private float endRotScale = 3f;

    [Header("Grown crop pulse indicator")] 
    [SerializeField] private Material animationMaterial;
    [SerializeField] private float pulseDuration = 1f;
    [SerializeField] private float pulseScaleMultiplier = 1.5f;
    [SerializeField] private float pulseStartingAlpha = 1f;
    [SerializeField] private float pulseFrequency = 0.5f;
    
    private bool IsOccupied => currentCrop != null;
    private bool isWatered = false;
    private bool isHovering = false;
    
    private GameObject currentCrop = null;
    private ItemData currentCropData = null;
    private float currentCropElapsedTime = 0f;

    private Image plotNeedsImg = null;
    
    private float elapsedWateredTime = 0f;
    private float maximumWateredTime = 0f;
    
    private Renderer currentCropRenderer;
    private Renderer plotRenderer;

    private GameObject pulseVisual;
    private Renderer pulseRenderer;
    private Vector3 initialPulseScale;
    private float pulseElapsedTime = 0f;

    private bool IsGrown => Math.Floor(currentCropElapsedTime / growTime) > 0;
    private bool IsRotting => Math.Floor(currentCropElapsedTime / (growTime + timeUntilRotStart)) > 0;
    private bool IsFullyRot => Math.Floor(currentCropElapsedTime / (growTime + timeUntilRotStart + rotTime)) > 0;

    private void Awake()
    {
        if (plotRenderer == null)
        {
            plotRenderer = GetComponentInChildren<Renderer>();
        }

        if (plotNeeds != null)
        {
            plotNeedsImg = GetComponentInChildren<Image>();
        }
        SetupPulseVisual();
        ResetColor();
    }

    private void SetupPulseVisual()
    {
        if (plotRenderer == null)
        {
            return;
        }

        pulseVisual = Instantiate(plotRenderer.gameObject, plotRenderer.transform.parent);
        pulseVisual.name = "CropGrownPulse";

        pulseRenderer = pulseVisual.GetComponent<Renderer>();
        if (animationMaterial != null && pulseRenderer != null)
        {
            pulseRenderer.material = animationMaterial;
        }

        initialPulseScale = pulseVisual.transform.localScale;
        pulseVisual.SetActive(false);
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

        bool isSeasonRight = currentCropData != null && SeasonManager.Instance.GetSeasonalCrop() == currentCropData;
        
        HandleCropNeeds(isSeasonRight);
        
        if (isSeasonRight)
        {
            if (IsOccupied && !IsGrown && currentCrop != null && isWatered)
            {
                GrowCrop();
            }
        }
        
        if (IsOccupied && IsGrown && !IsRotting)
        {
            currentCropElapsedTime += Time.deltaTime;
        }
        
        if (IsOccupied && IsGrown && IsRotting && currentCropRenderer != null)
        {
            RotCrop();
        }

        if (IsOccupied && IsFullyRot && currentCrop != null)
        {
            DestroyCrop();
        }
        
        HandleGrownPulse();
    }

    private float EaseInQuint(float progress)
    {
        return Mathf.Pow(progress, 5f);
    }
    
    private float EaseOutQuint(float progress)
    {
        return 1f - Mathf.Pow(1 - progress, 5f);
    }

    private void HandleGrownPulse()
    {
        bool shouldPulse = IsOccupied && IsGrown && !IsRotting;
        if (pulseVisual == null)
        {
            return;
        }
        if (shouldPulse)
        {
            if (!pulseVisual.activeSelf)
            {
                pulseVisual.SetActive(true);
                pulseElapsedTime = 0f;
            }

            pulseElapsedTime += Time.deltaTime;

            float progress = (pulseElapsedTime % (pulseDuration + pulseFrequency)) / pulseDuration;
            float easeOutQuintProgress = EaseOutQuint(progress);

            float currentMultiplier = Mathf.Lerp(1f, pulseScaleMultiplier, easeOutQuintProgress);
            pulseVisual.transform.localScale = new Vector3(
                initialPulseScale.x * currentMultiplier,
                initialPulseScale.y / 2,
                initialPulseScale.z * currentMultiplier
            );

            if (pulseRenderer != null)
            {
                float easeInQuintProgress = EaseInQuint(progress);
                Color color = pulseRenderer.material.color;
                color.a = Mathf.Lerp(pulseStartingAlpha, 0f, progress);
                pulseRenderer.material.color = color;
            }
        }
        else
        {
            if (pulseVisual.activeSelf)
            {
                pulseVisual.SetActive(false);
            }
        }
    }

    private void HandleCropNeeds(bool isSeasonRight)
    {
        if (plotNeeds == null || plotNeedsImg == null) return;

        if (!IsOccupied || currentCropData == null)
        {
            plotNeeds.enabled = false;
            return;
        }

        Sprite iconToShow = null;

        if (!isSeasonRight)
        {
            iconToShow = currentCropData.requiredSeasonIcon;
        }
        else if (!isWatered && !IsGrown)
        {
            iconToShow = waterDropIcon;
        } 
        else if (IsRotting)
        {
            iconToShow = exclamationMarkIcon;
        }

        if (iconToShow != null)
        {
            plotNeeds.enabled = true;
            plotNeedsImg.sprite = iconToShow;

            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(smallScale, bigScale, wave);
            plotNeeds.transform.localScale = Vector3.one * scale;
        }
        else
        {
            plotNeeds.enabled = false;
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
        float t = (currentCropElapsedTime - growTime - timeUntilRotStart) / rotTime;
        currentCropRenderer.material.color = Color.Lerp(Color.white, currentCropData.rotColor, t);

        float scale = Mathf.Lerp(endScale, endRotScale, t);
        currentCrop.transform.localScale = Vector3.one * scale;

        float rotation = Mathf.Lerp(0f, endRotation, t);
        currentCrop.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
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
        if (Inventory.Instance.HoldingItem == wateringCan && !isWatered)
        {
            isWatered = true;
            maximumWateredTime = SeasonManager.Instance.GetDryOutTime();
            SetColor();
        } 
        else if (Inventory.Instance.HoldingItem == shovel && IsOccupied)
        {
            RemoveCurrentCrop();
        }
        else if (Inventory.Instance.HoldingItem != wateringCan && Inventory.Instance.HoldingItem != shovel && Inventory.Instance.HoldingItem != null && !IsOccupied)
        {
            GameObject cropClone = Instantiate(Inventory.Instance.HoldingItem.prefab, transform);
            currentCrop = cropClone;
            currentCropData = Inventory.Instance.HoldingItem;
            currentCropRenderer = cropClone.GetComponent<Renderer>();

            if (currentCropData != SeasonManager.Instance.GetSeasonalCrop())
            {
                HUD.Instance.ShowNotification("This crop will not grow in this season.");
            }
        }
        else if (IsOccupied && IsGrown && !IsFullyRot)
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
