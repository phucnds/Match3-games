using UnityEngine;

public class GemGridVisual
{
    private Transform transform;
    private GemGrid gemGrid;

    public GemGridVisual(Transform transform, GemGrid gemGrid)
    {
        this.transform = transform;
        this.gemGrid = gemGrid;

        gemGrid.OnDestroyed += GemGrid_OnDestroyed;
    }

    private void GemGrid_OnDestroyed(bool playFx)
    {
        if (playFx)
        {
            transform.GetComponent<GemGridAnimation>().PlayEffect();
        }
        else
        {
            transform.GetComponent<GemGridAnimation>().Release();
        }
    }

    public void Update()
    {
        Vector3 targetPosition = gemGrid.GetWorldPosition();
        Vector3 moveDir = (targetPosition - transform.position);
        float moveSpeed = 8f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
