using UnityEngine;
/// <summary>
/// Bad insect destroys the players crop.
/// When the insect arrives, it eats from the crop, and destroys it after a set duration.
/// When the insect is killed on purpose, the player gains a point.
/// </summary>
public class BadInsect : Insect
{
    [SerializeField] private float eatDuration = 4f;

    private float _elapsedEatingTime;
    private bool _isEating;

    protected override void OnArrived()
    {
        _isEating = true;
        _elapsedEatingTime = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (_isEating && !IsTutorialInsect)
        {
            _elapsedEatingTime += Time.deltaTime;

            
            // When the insect exceeds its eat duration, it destroys itself and the crop. The player does not gain a point for the insect dying.
            if (_elapsedEatingTime >= eatDuration)
            {
                DestroyCrop();
            }
        }
    }

    public override float GetProgress()
    {
        return Mathf.Clamp01(_elapsedEatingTime / eatDuration);
    }

    public override void Kill(bool affectScore)
    {
        base.Kill(affectScore);

        if (affectScore)
        {
            HUD.Instance.ShowNotification($"+1 For killing a bad insect!", Color.darkGreen);
            GameManager.Instance.BadInsectKillBonus();
        }
    }

    private void DestroyCrop()
    {
        TargetPlot.DestroyCrop();
        Destroy(gameObject);
    }
}
