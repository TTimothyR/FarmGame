using UnityEngine;

public class GoodInsect : Insect
{
    [SerializeField] private float pollinateDuration = 3f;
    [SerializeField] private float pollinateBoost = 1.2f;

    private float _elapsedPollinateTime;
    private bool _isPollinating;

    protected override void OnArrived()
    {
        _isPollinating = true;
        _elapsedPollinateTime = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (_isPollinating)
        {
            _elapsedPollinateTime += Time.deltaTime;

            if (_elapsedPollinateTime >= pollinateDuration)
            {
                base.Kill(false);
            }
        }
    }

    public override float GetProgress()
    {
        return Mathf.Clamp01(_elapsedPollinateTime / pollinateDuration);
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
