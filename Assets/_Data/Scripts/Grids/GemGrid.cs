using System;
using UnityEngine;

public class GemGrid
{
    public Action<bool> OnDestroyed;

    private GemSO gem;
    private int x;
    private int y;
    private bool isDestroyed;

    public int color = -1;

    public GemGrid(GemSO gem, int x, int y)
    {
        this.gem = gem;
        this.x = x;
        this.y = y;

        isDestroyed = false;
    }

    public GemSO GetGem()
    {
        return gem;
    }

    public void SetGemSO(GemSO gemSO)
    {
        gem = gemSO;
    }

    public Vector3 GetWorldPosition()
    {
        return new Vector3(x, y);
    }

    public void SetGemXY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Destroy(bool playFx = true)
    {
        isDestroyed = true;
        OnDestroyed?.Invoke(playFx);
    }

    public override string ToString()
    {
        return isDestroyed.ToString();
    }
}
