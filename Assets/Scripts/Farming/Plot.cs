using System;
using UnityEngine;
using UnityEngine.UI;


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

    private bool IsGrown => Math.Floor(_currentCropElapsedTime / growTime) > 0;
    private bool IsRotting => Math.Floor(_currentCropElapsedTime / (growTime + timeUntilRotStart)) > 0;
    private bool IsFullyRot => Math.Floor(_currentCropElapsedTime / (growTime + timeUntilRotStart + rotTime)) > 0;
    
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
        
        if (isSeasonRight)
        {
            if (IsOccupied && !IsGrown && _currentCrop != null && _isWatered)
            {
                GrowCrop();
            }
        }
        
        if (IsOccupied && IsGrown && !IsRotting)
        {
            _currentCropElapsedTime += Time.deltaTime;
        }
        
        if (IsOccupied && IsGrown && IsRotting && _cropMaterial != null && _currentCrop != null)
        {
            RotCrop();
        }

        if (IsOccupied && IsFullyRot && _currentCrop != null)
        {
            DestroyCrop();
        }
        
        HandleGrownPulse();
    }

    private float EaseOutQuint(float progress)
    {
        return 1f - Mathf.Pow(1 - progress, 5f);
    }

    private void HandleGrownPulse()
    {
        bool shouldPulse = IsOccupied && IsGrown && !IsRotting;
        if (_pulseVisual == null)
        {
            return;
        }
        if (shouldPulse)
        {
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
        else if (IsRotting)
        {
            iconToShow = exclamationMarkIcon;
        }

        if (iconToShow != null)
        {
            plotNeeds.enabled = true;
            _plotNeedsImg.sprite = iconToShow;

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

        if (_currentInsect is GoodInsect goodInsect)
        {
            boostMultiplier = goodInsect.GetBoost();
        }
        
        _currentCropElapsedTime += Time.deltaTime * boostMultiplier;
        float t = _currentCropElapsedTime / growTime;
        float scale = Mathf.Lerp(startScale, endScale, t);
        _currentCrop.transform.localScale = Vector3.one * scale;
    }

    private void RotCrop()
    {
        float boostMultiplier = 1;

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
        if (TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.TutorialSteps step = TutorialManager.Instance.CurrentStep;

            if (step != TutorialManager.TutorialSteps.PlantRadish &&
                step != TutorialManager.TutorialSteps.PlantPumpkin)
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
