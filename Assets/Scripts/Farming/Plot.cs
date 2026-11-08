using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Manages all the functionality of a plot.
/// Is a plot watered? Is there an insect on the plot? Is there a crop on the plot?
/// Visual cues of the plot are also managed here.
/// </summary>
public class Plot : MonoBehaviour
{
    [SerializeField] private Sprite waterDropIcon;
    [SerializeField] private Sprite exclamationMarkIcon;
    [SerializeField] private ParticleSystem harvestEffectTemplate;
    
    [Header("Needs Icon Animation")]
    [SerializeField] private Canvas plotNeeds;
    [SerializeField] private Canvas insectIndicator;
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
    
    public bool IsOccupied => _currentCrop != null;
    public bool HasInsect => _currentInsect != null;
    private bool IsOnHarvestStep => TutorialManager.Instance.CurrentStep == TutorialManager.TutorialSteps.HarvestCrop;
    private bool _isWatered;
    private bool _isHovering;
    
    private GameObject _currentCrop;
    private Insect _currentInsect;
    private ItemData _currentCropData;
    private float _currentCropElapsedTime;

    private Image _plotNeedsImg;
    private Image _insectIndicatorImg;
    
    private float _elapsedWateredTime;
    private float _maximumWateredTime;
    
    private Renderer _currentCropRenderer;
    private Renderer _plotRenderer;

    private Material _plotMaterial;
    private Material _cropMaterial;

    private GameObject _pulseVisual;
    private Renderer _pulseRenderer;
    private Vector3 _initialPulseScale;
    private float _pulseElapsedTime;

    /// <summary>
    /// A crop is fully grown when its total elapsed time is greater than the time it needs to grow.
    /// A crop is rotting when it has fully grown, and it has exceeded the time before it starts rotting.
    /// A crop is fully rotten when its elapsed time is greater than the time needed for the 2 previous stages, and the time needed to fully rot.
    /// </summary>
    private bool IsGrown => _currentCropElapsedTime >= growTime;
    private bool IsRotting => _currentCropElapsedTime >= (growTime + timeUntilRotStart);
    private bool IsFullyRot => _currentCropElapsedTime >= (growTime + timeUntilRotStart + rotTime);
    
    private void Awake()
    {
        if (_plotRenderer == null)
        {
            _plotRenderer = GetComponentInChildren<Renderer>();
        }

        if (plotNeeds != null)
        {
            _plotNeedsImg = plotNeeds.GetComponentInChildren<Image>();
        }

        if (insectIndicator != null)
        {
            _insectIndicatorImg = insectIndicator.GetComponentInChildren<Image>();
        }

        if (_plotRenderer != null)
        {
            _plotMaterial = _plotRenderer.material;
        }
        
        SetupPulseVisual();
        ResetColor();
    }

    private void SetupPulseVisual()
    {
        if (_plotRenderer == null)
        {
            return;
        }

        // Creates a copy of the plot renderer used for the white pulse effect when a crop is fully grown
        _pulseVisual = Instantiate(_plotRenderer.gameObject, _plotRenderer.transform.parent);
        _pulseVisual.name = "CropGrownPulse";

        _pulseRenderer = _pulseVisual.GetComponent<Renderer>();
        if (animationMaterial != null && _pulseRenderer != null)
        {
            _pulseRenderer.material = animationMaterial;
        }

        _initialPulseScale = _pulseVisual.transform.localScale;
        _pulseVisual.SetActive(false);
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }
        if (_isWatered)
        {
            DryOutOverTime();
        }

        bool isSeasonRight = _currentCropData != null && SeasonManager.Instance.GetSeasonalCrop() == _currentCropData;
        
        HandleCropNeeds(isSeasonRight);
        HandleInsectIndicator();
        
        // A crop only grows in the matching season.
        if (isSeasonRight)
        {
            // Additionally, in order to grow, the plot must be occupied by a crop, it must not already be grown and the plot must be watered.
            if (IsOccupied && !IsGrown && _currentCrop != null && _isWatered)
            {
                GrowCrop();
            }
        }
        
        // If a crop is not fully grown, the elapsed time is updated in the GrowCrop method.
        // If the crop is rotting, the elapsed time is updated in the RotCrop method.
        if (IsOccupied && IsGrown && !IsRotting)
        {
            _currentCropElapsedTime += Time.deltaTime;
        }
        
        // A crop will not rot when the required tutorial step is to harvest the crop.
        // Letting the crop rot and disappear would soft lock the player.
        if (IsOccupied && IsGrown && IsRotting && _cropMaterial != null && _currentCrop != null && !IsOnHarvestStep)
        {
            RotCrop();
        }

        if (IsOccupied && IsFullyRot && _currentCrop != null && !IsOnHarvestStep)
        {
            DestroyCrop();
        }
        
        HandleGrownPulse();
    }

    // Used to create a nice effect for the Grown Crop Pulse Indicator.
    private float EaseOutQuint(float progress)
    {
        return 1f - Mathf.Pow(1 - progress, 5f);
    }

    private void HandleGrownPulse()
    {
        bool shouldPulse = IsOccupied && IsGrown && (!IsRotting || !IsOnHarvestStep);
        if (_pulseVisual == null)
        {
            return;
        }
        if (shouldPulse)
        {
            // If the animation wasn't playing already, this resets the elapsed time.
            if (!_pulseVisual.activeSelf)
            {
                _pulseVisual.SetActive(true);
                _pulseElapsedTime = 0f;
            }

            _pulseElapsedTime += Time.deltaTime;

            float progress = (_pulseElapsedTime % (pulseDuration + pulseFrequency)) / pulseDuration;
            float easeOutQuintProgress = EaseOutQuint(progress);

            float currentMultiplier = Mathf.Lerp(startScale, pulseScaleMultiplier, easeOutQuintProgress);
            _pulseVisual.transform.localScale = new Vector3(
                _initialPulseScale.x * currentMultiplier,
                _initialPulseScale.y / 2,
                _initialPulseScale.z * currentMultiplier
            );

            // Fades the visual from opaque to fully transparent.
            if (_pulseRenderer != null)
            {
                Color color = _pulseRenderer.material.color;
                color.a = Mathf.Lerp(pulseStartingAlpha, 0f, progress);
                _pulseRenderer.material.color = color;
            }
        }
        else
        {
            if (_pulseVisual.activeSelf)
            {
                _pulseVisual.SetActive(false);
            }
        }
    }

    private void HandleInsectIndicator()
    {
        if (insectIndicator == null || _insectIndicatorImg == null)
        {
            return;
        }
        
        if (!HasInsect)
        {
            insectIndicator.enabled = false;
            return;
        }

        if (!_currentInsect.HasArrived)
        {
            insectIndicator.enabled = false;
            return;
        }

        _insectIndicatorImg.color = _currentInsect is GoodInsect ? Color.lightGreen : Color.darkRed;
        insectIndicator.enabled = true;
        _insectIndicatorImg.fillAmount = _currentInsect.GetProgress();
    }

    private void HandleCropNeeds(bool isSeasonRight)
    {
        if (plotNeeds == null || _plotNeedsImg == null)
        {
            return;
        }

        if (!IsOccupied || _currentCropData == null)
        {
            plotNeeds.enabled = false;
            return;
        }

        Sprite iconToShow = null;

        if (!isSeasonRight)
        {
            iconToShow = _currentCropData.requiredSeasonIcon;
        }
        else if (!_isWatered && !IsGrown)
        {
            iconToShow = waterDropIcon;
        } 
        else if (IsRotting && !IsOnHarvestStep)
        {
            iconToShow = exclamationMarkIcon;
        }

        if (iconToShow != null)
        {
            plotNeeds.enabled = true;
            _plotNeedsImg.sprite = iconToShow;

            
            // Makes the icon grow bigger and smaller over and over to emphasize.
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(smallScale, bigScale, wave);
            plotNeeds.transform.localScale = Vector3.one * scale;
        }
        else
        {
            plotNeeds.enabled = false;
        }
    }

    // Used in the plot dry out color to make the color change more noticeable 
    private float EaseInSine(float progress)
    {
        return 1 - Mathf.Cos((progress * Mathf.PI) / 2f);
    }

    private void DryOutOverTime()
    {
        if (_plotMaterial == null)
        {
            return;
        }
        _elapsedWateredTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedWateredTime / _maximumWateredTime);
        float easedProgress = EaseInSine(progress);

        Color currentColor = Color.Lerp(wateredColor, baseColor, easedProgress);
        _plotMaterial.color = currentColor;
        if (_elapsedWateredTime >= _maximumWateredTime)
        {
            ResetWatering();
        }
    }

    private void GrowCrop()
    {
        float boostMultiplier = 1;

        // If there's a good insect on the crop, it grows faster.
        if (_currentInsect is GoodInsect goodInsect)
        {
            boostMultiplier = goodInsect.GetBoost();
        }
        
        // Linearly increase the crops scale based on the growth.
        _currentCropElapsedTime += Time.deltaTime * boostMultiplier;
        float t = _currentCropElapsedTime / growTime;
        float scale = Mathf.Lerp(startScale, endScale, t);
        _currentCrop.transform.localScale = Vector3.one * scale;
    }

    private void RotCrop()
    {
        float boostMultiplier = 1;

        // If there's a good insect on the crop, it rots slower.
        if (_currentInsect is GoodInsect goodInsect)
        {
            boostMultiplier = goodInsect.GetBoost();
        }
        _currentCropElapsedTime += Time.deltaTime / boostMultiplier;
        float t = (_currentCropElapsedTime - growTime - timeUntilRotStart) / rotTime;
        _cropMaterial.color = Color.Lerp(Color.white, _currentCropData.rotColor, t);

        float scale = Mathf.Lerp(endScale, endRotScale, t);
        _currentCrop.transform.localScale = Vector3.one * scale;

        float rotation = Mathf.Lerp(0f, endRotation, t);
        _currentCrop.transform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
    }

    private void ResetWatering()
    {
        _isWatered = false;
        _elapsedWateredTime = 0f;
        _maximumWateredTime = 0f;
        SetColor();
    }

    private void RemoveCurrentCrop()
    {
        Destroy(_currentCrop);
        _currentCropRenderer = null;
        _cropMaterial = null;
        _currentCropElapsedTime = 0f;
        _currentCropData = null;

        if (_currentInsect != null)
        {
            _currentInsect.Kill(false);
        }
        ResetWatering();
    }

    public void DestroyCrop()
    {
        if (IsFullyRot)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.WaitForCropRot);
        }
        RemoveCurrentCrop();
        GameManager.Instance.LoseLife();
    }

    private void WaterCrop()
    {
        TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.WaterCrop);
        AudioManager.Instance.Play(AudioManager.SoundType.WaterCrop);
        _isWatered = true;
        _maximumWateredTime = SeasonManager.Instance.GetDryOutTime();
        SetColor();
    }

    private void PlantCrop()
    {
        // During the tutorial, the player can only plant the asked seed, and cannot plant anything when this isn't required.
        if (TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.TutorialSteps step = TutorialManager.Instance.CurrentStep;

            if (step != TutorialManager.TutorialSteps.PlantRadish &&
                step != TutorialManager.TutorialSteps.PlantPumpkin && step != TutorialManager.TutorialSteps.WaterRadish2)
            {
                HUD.Instance.FollowTutorialNotification();
                return;
            }

            if (step == TutorialManager.TutorialSteps.PlantPumpkin &&
                Inventory.Instance.HoldingItem != Inventory.Instance.PumpkinSeed)
            {
                HUD.Instance.FollowTutorialNotification();
                return;
            }
            
            if (step is TutorialManager.TutorialSteps.PlantRadish
                    or TutorialManager.TutorialSteps.WaterRadish2
                && Inventory.Instance.HoldingItem != Inventory.Instance.RadishSeed)
            {
                HUD.Instance.FollowTutorialNotification();
                return;
            }
        }

        if (Inventory.Instance.HoldingItem.prefab == null)
        {
            return;
        }
        
        AudioManager.Instance.Play(AudioManager.SoundType.PlantSeed);
        GameObject cropClone = Instantiate(Inventory.Instance.HoldingItem.prefab, transform);
        _currentCrop = cropClone;
        _currentCropData = Inventory.Instance.HoldingItem;
        _currentCropRenderer = cropClone.GetComponent<Renderer>();
        
        if (_currentCropRenderer != null)
        {
            _cropMaterial = _currentCropRenderer.material;
        }
            
        if (Inventory.Instance.HoldingItem == Inventory.Instance.RadishSeed)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.PlantRadish);
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.WaterRadish2);
        } 
        else if (Inventory.Instance.HoldingItem == Inventory.Instance.PumpkinSeed)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.PlantPumpkin);
        }

        if (_currentCropData != SeasonManager.Instance.GetSeasonalCrop())
        {
            HUD.Instance.ShowNotification("This crop will not grow in this season.");
        }
    }

    private void HarvestCrop()
    {
        // During the tutorial, you cannot harvest the crop that should rot to avoid soft lock.
        if (TutorialManager.Instance.CurrentStep == TutorialManager.TutorialSteps.WaitForCropRot)
        {
            HUD.Instance.FollowTutorialNotification();
            return;
        }
        if (TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.HarvestCrop);
        }

        if (harvestEffectTemplate != null)
        {
            const float particleHeightOffset = 1f;
            Instantiate(harvestEffectTemplate, _currentCrop.transform.position + new Vector3(0f, particleHeightOffset, 0f),
                harvestEffectTemplate.transform.localRotation);
        }
        AudioManager.Instance.Play(AudioManager.SoundType.Harvest);

        RemoveCurrentCrop();
        GameManager.Instance.GainScore();
    }

    public void OnHoverEnter()
    {
        _isHovering = true;
        SetColor();
    }

    public void OnHoverExit()
    {
        ResetColor();
    }
    

    public void Interact()
    {
        if (Inventory.Instance.HoldingItem == Inventory.Instance.WateringCan && !_isWatered)
        {
            WaterCrop();
        } 
        else if (Inventory.Instance.HoldingItem == Inventory.Instance.Shovel && IsOccupied)
        {
            if (TutorialManager.Instance.IsTutorialActive)
            {
                // During the tutorial, you can only destroy a crop with the shovel during its step to avoid soft lock.
                if (TutorialManager.Instance.CurrentStep != TutorialManager.TutorialSteps.DestroyCrop)
                {
                    HUD.Instance.FollowTutorialNotification();
                    return;
                }
            }
            TutorialManager.Instance.StepFinished(TutorialManager.TutorialSteps.DestroyCrop);
            RemoveCurrentCrop();
        }
        else if (Inventory.Instance.HoldingItem != Inventory.Instance.WateringCan && Inventory.Instance.HoldingItem != Inventory.Instance.Shovel && Inventory.Instance.HoldingItem != null && !IsOccupied)
        {
            PlantCrop();
        }
        else if (IsOccupied && IsGrown && !IsFullyRot)
        {
            HarvestCrop();
        }
    }

    public void AddInsect(Insect newInsect)
    {
        if (_currentInsect != null)
        {
            return;
        }

        _currentInsect = newInsect;
    }

    private void SetColor()
    {
        if (_plotMaterial == null)
        {
            return;
        }
        Color targetColor = _isWatered ? wateredColor : baseColor;
        _plotMaterial.color = _isHovering ? Color.Lerp(targetColor, Color.white, 0.2f) : targetColor;
    }

    private void ResetColor()
    {
        _isHovering = false;
        SetColor();
    }
}
