using UnityEngine;

public abstract class Insect : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] private ParticleSystem splatInsectEffectTemplate;

    protected Plot TargetPlot;
    protected bool IsTutorialInsect;
    public bool HasArrived { get; private set; }

    private const float ArriveThreshold = 0.2f;

    public void Initialize(Plot plot, bool tutorialInsect)
    {
        TargetPlot = plot;
        IsTutorialInsect = tutorialInsect;
    }

    protected virtual void Update()
    {
        if (TargetPlot == null || HasArrived)
        {
            return;
        }

        if (!GameManager.Instance.GameStarted)
        {
            return;
        }

        Vector3 targetPosition = TargetPlot.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.LookAt(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < ArriveThreshold)
        {
            HasArrived = true;
            OnArrived();
        }
    }

    protected abstract void OnArrived();
    public abstract float GetProgress();

    public virtual void Kill(bool affectScore)
    {
        const float particleHeightOffset = 1.5f;
        if (affectScore)
        {
            AudioManager.Instance.Play(AudioManager.SoundType.SplatInsect);

            if (splatInsectEffectTemplate != null)
            {
                Instantiate(splatInsectEffectTemplate, transform.position + new Vector3(0f, particleHeightOffset, 0f),
                    splatInsectEffectTemplate.transform.localRotation);
            }
        }
        Destroy(gameObject);
    }
}
