using UnityEngine;

public class GoodInsect : Insect
{
    // [SerializeField] private int bonusScore = 3;
    [SerializeField] private float pollinateDuration = 3f;
    [SerializeField] private float pollinateBoost = 1.2f;

    private float elapsedPollinateTime = 0f;
    private bool isPollinating = false;

    protected override void OnArrived()
    {
        isPollinating = true;
        elapsedPollinateTime = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (isPollinating)
        {
            elapsedPollinateTime += Time.deltaTime;

            if (elapsedPollinateTime >= pollinateDuration)
            {
                base.Kill(false);
            }
        }
    }

    public override float GetProgress()
    {
        return Mathf.Clamp01(elapsedPollinateTime / pollinateDuration);
    }

    public override void Kill(bool affectScore)
    {
        base.Kill(affectScore);

        if (affectScore)
        {
            HUD.Instance.ShowNotification($"-1 For killing a good insect!", Color.darkRed);
            GameManager.Instance.GoodInsectKillPunishment();
        }
    }

    public float GetBoost()
    {
        return pollinateBoost;
    }
}
