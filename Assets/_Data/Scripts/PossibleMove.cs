using System.Collections.Generic;
using UnityEngine;

public class PossibleMove
{
    public int startX;
    public int startY;
    public int endX;
    public int endY;
    public List<List<GemGridPosition>> allLinkedGemGridPositionList;

    public PossibleMove() { }

    public PossibleMove(int startX, int startY, int endX, int endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;
    }

    public int GetTotalMatchAmount()
    {
        int total = 0;
        foreach (List<GemGridPosition> linkedGemGridPositionList in allLinkedGemGridPositionList)
        {
            total += linkedGemGridPositionList.Count;
        }
        return total;
    }

    public override string ToString()
    {
        return startX + ", " + startY + " => " + endX + ", " + endY + " == " + allLinkedGemGridPositionList?.Count;
    }
}
