using UnityEngine;

public abstract class Insect : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;

    protected Plot targetPlot;
    public bool HasArrived { get; private set; } = false;

    public void Initialize(Plot plot)
    {
        targetPlot = plot;
    }

    protected virtual void Update()
    {
        if (targetPlot == null || HasArrived)
        {
            return;
        }

        Vector3 targetPosition = targetPlot.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.LookAt(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            HasArrived = true;
            OnArrived();
        }
    }

    protected abstract void OnArrived();
    public abstract float GetProgress();

    public virtual void Kill()
    {
        Destroy(gameObject);
    }
}
