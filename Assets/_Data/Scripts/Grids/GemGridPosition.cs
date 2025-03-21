using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SquareTypes
{
    EmptySquare = 0,
    SolidBlock = 1,
    NONE
}

public class GemGridPosition
{
    private Grid<GemGridPosition> grid;
    private int x;
    private int y;

    private SquareTypes type;
    private GemGrid gemGrid;
    private Texture2D texture;

    public GemGridPosition(Grid<GemGridPosition> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        type = SquareTypes.EmptySquare;
    }

    public void SetGemGrid(GemGrid gemGrid)
    {
        this.gemGrid = gemGrid;
        grid.TriggerGridObjectChanged(x, y);
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y);
    }

    public GemGrid GetGemGrid()
    {
        return gemGrid;
    }

    public void ClearGemGrid()
    {
        gemGrid = null;
    }

    public void DestroyGem(bool playFx = true)
    {
        gemGrid?.Destroy(playFx);
        grid.TriggerGridObjectChanged(x, y);
    }

    public bool HasGemGrid()
    {
        return gemGrid != null;
    }

    public bool IsEmpty()
    {
        return gemGrid == null;
    }

    public void SetSquareType(SquareTypes type)
    {
        this.type = type;
    }

    public bool IsBlock()
    {
        return type == SquareTypes.SolidBlock;
    }

    public void SetTexture(Texture2D texture)
    {
        this.texture = texture;
    }

    public Texture2D GetTexture()
    {
        return texture;
    }

    public override string ToString()
    {
        // return gemGrid?.ToString();

        return $"x: {x} - y: {y}";
    }

    public bool WillCauseMatch3(int col)
    {

        if (CheckColorMatch(GetNeighborLeft(), col) && CheckColorMatch(GetNeighborLeft()?.GetNeighborLeft(), col))
            return true;
        if (CheckColorMatch(GetNeighborLeft(), col) && CheckColorMatch(GetNeighborRight(), col))
            return true;
        if (CheckColorMatch(GetNeighborRight(), col) && CheckColorMatch(GetNeighborRight()?.GetNeighborRight(), col))
            return true;


        if (CheckColorMatch(GetNeighborTop(), col) && CheckColorMatch(GetNeighborTop()?.GetNeighborTop(), col))
            return true;
        if (CheckColorMatch(GetNeighborTop(), col) && CheckColorMatch(GetNeighborBottom(), col))
            return true;
        if (CheckColorMatch(GetNeighborBottom(), col) && CheckColorMatch(GetNeighborBottom()?.GetNeighborBottom(), col))
            return true;

        return false;
    }

    public bool IsGemGridCenter()
    {
        if (IsBlock()) return false;

        if (GetNeighborLeft() != null && GetNeighborRight() != null) return true;

        if (GetNeighborTop() != null && GetNeighborBottom() != null) return true;

        return false;
    }

    public List<GemGridPosition> ListNeighbor()
    {
        List<GemGridPosition> lst = new List<GemGridPosition>();

        if (GetNeighborLeft() != null) lst.Add(GetNeighborLeft());
        if (GetNeighborRight() != null) lst.Add(GetNeighborRight());
        if (GetNeighborTop() != null) lst.Add(GetNeighborTop());
        if (GetNeighborBottom() != null) lst.Add(GetNeighborBottom());

        return lst;
    }

    private bool CheckColorMatch(GemGridPosition neighbor, int col)
    {
        return neighbor?.GetGemGrid()?.color == col;
    }

    public GemGridPosition GetNeighborBottom()
    {
        GemGridPosition neighbor = grid.GetGridObject(x, y - 1);
        return (neighbor == null || neighbor.IsBlock()) ? null : neighbor;
    }

    public GemGridPosition GetNeighborTop()
    {
        GemGridPosition neighbor = grid.GetGridObject(x, y + 1);
        return (neighbor == null || neighbor.IsBlock()) ? null : neighbor;
    }

    public GemGridPosition GetNeighborLeft()
    {
        GemGridPosition neighbor = grid.GetGridObject(x - 1, y);
        return (neighbor == null || neighbor.IsBlock()) ? null : neighbor;
    }

    public GemGridPosition GetNeighborRight()
    {
        GemGridPosition neighbor = grid.GetGridObject(x + 1, y);
        return (neighbor == null || neighbor.IsBlock()) ? null : neighbor;
    }
}
