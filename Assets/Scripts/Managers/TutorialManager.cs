using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    public enum TutorialSteps
    {
        MoveAround,
        PlantRadish,
        WaterCrop,
        HarvestCrop,
        WaterRadish2,
        WaitForCropRot,
        KillInsect,
        PlantPumpkin,
        DestroyCrop,
        Count,
    }

    [SerializeField] private TextMeshProUGUI tutorialInstructionText;

    public TutorialSteps CurrentStep { get; private set; } = 0;
    public bool IsTutorialActive { get; private set; }

    private readonly string[] _instructions = new string[]
    {
        "Walk around using WASD.",
        "Equip your Radish Seed from the hotbar and click a plot to plant it.",
        "Equip your Watering Can and click the plot to water it. \n Plots dry out faster or slower, depending on the season!",
        "Wait until the crop is fully grown, and click it to harvest. It gives you one point towards the quota. \n If you miss the quota, it's game over.",
        "Plant another Radish and water it.",
        "Wait for the crop to fully rot away. You lose 1 life. Don't lose all your lives!",
        "An insect approaches. Equip your Shovel and click the insect to kill it! You shouldn't kill them all though...",
        "Equip your Pumpkin Seed and plant it.",
        "Pumpkin doesn't grow in this season. Equip your Shovel and click the plot to get rid of it!"
    };

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

    public void StartTutorial()
    {
        if (tutorialInstructionText == null)
        {
            return;
        }
        IsTutorialActive = true;
        tutorialInstructionText.enabled = true;
        CurrentStep = TutorialSteps.MoveAround;
        ShowCurrentStep();
    }

    public void StepFinished(TutorialSteps completedStep)
    {
        if (!IsTutorialActive || completedStep != CurrentStep)
        {
            return;
        }

        CurrentStep++;

        if (CurrentStep == TutorialSteps.KillInsect)
        {
            InsectManager.Instance.SpawnTutorialInsect();
        }

        if ((int)CurrentStep < (int)TutorialSteps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            FinishTutorial();
        }
    }
    
    private void ShowCurrentStep()
    {
        if (tutorialInstructionText == null)
        {
            return;
        }
        tutorialInstructionText.text = $"({(int)CurrentStep + 1}/{(int)TutorialSteps.Count}) - {_instructions[(int)CurrentStep]}";
    }

    private void FinishTutorial()
    {
        if (tutorialInstructionText == null)
        {
            return;
        }
        IsTutorialActive = false;
        tutorialInstructionText.enabled = false;
        
        HUD.Instance.ShowNotification("Tutorial complete! Spring has now started.", Color.green);
        GameManager.Instance.ResetTutorialValues();
    }
}
