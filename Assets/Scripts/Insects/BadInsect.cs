using UnityEngine;

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
