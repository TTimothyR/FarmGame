using UnityEngine;

public class BadInsect : Insect
{
    [SerializeField] private float eatDuration = 4f;

    private float elapsedEatingTime = 0f;
    private bool isEating = false;

    protected override void OnArrived()
    {
        isEating = true;
        elapsedEatingTime = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (isEating && !isTutorialInsect)
        {
            elapsedEatingTime += Time.deltaTime;

            if (elapsedEatingTime >= eatDuration)
            {
                DestroyCrop();
            }
        }
    }

    public override float GetProgress()
    {
        return Mathf.Clamp01(elapsedEatingTime / eatDuration);
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
        targetPlot.DestroyCrop();
        Destroy(gameObject);
    }
}
