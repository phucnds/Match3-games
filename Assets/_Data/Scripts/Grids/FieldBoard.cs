using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldBoard : MonoBehaviour
{
    private LevelSO levelData;

    private int gridWidth;
    private int gridHeight;
    private Grid<GemGridPosition> grid;

    private bool match4Explosions;

    public Action<GemGridPosition> OnGemGridPositionDestroyed;
    public Action<GemGrid, GemGridPosition> OnNewGemGridSpawned;

    private void Awake()
    {
        LevelManager.Instance.OnStateChanged += LevelManager_OnStateChanged;
    }

    private void LevelManager_OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.PrepareGame:
                SetLevel();
                break;

            case GameState.Shuffle:
                CreateFieldBoardData();
                break;
        }
    }


    public void SetLevel()
    {
        levelData = LevelManager.Instance.GetCurrentLevelData();

        if (levelData == null)
        {
            Debug.Log("No data");
            return;
        }

        gridWidth = levelData.width;
        gridHeight = levelData.height;

        grid = new Grid<GemGridPosition>(gridWidth, gridHeight, 1f, Vector3.zero, (Grid<GemGridPosition> g, int x, int y) => new GemGridPosition(g, x, y));

        CreateFieldBoardData();
    }

    public void CreateFieldBoardData()
    {
        // Debug.Log("Create FieldBoard Data");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int index = y * gridWidth + x;
                var levelGrid = levelData.levelGridPositionList[index];

                grid.GetGridObject(x, y).SetSquareType(levelGrid.type);
                grid.GetGridObject(x, y).SetTexture(levelGrid.texture);

                if (levelGrid.type == SquareTypes.SolidBlock) continue;

                int color = GenColor(grid.GetGridObject(x, y), levelData.gemList.Count);

                GemSO gem = levelData.gemList[color];
                GemGrid gemGrid = new GemGrid(gem, x, y);
                gemGrid.color = color;

                grid.GetGridObject(x, y).SetGemGrid(gemGrid);

            }
        }

        if (GetAllPossibleMoves().Count == 0)
        {
            CreatePossibleMove(levelData.gemList.Count);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid.GetGridObject(x, y).IsBlock()) continue;

                    int color = grid.GetGridObject(x, y).GetGemGrid().color;
                    GemSO gemSO = levelData.gemList[color];
                    grid.GetGridObject(x, y).GetGemGrid().SetGemSO(gemSO);
                }
            }

        }

        LevelManager.Instance.SetupVisual();
    }

    private void CreatePossibleMove(int colorLimit)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GemGridPosition currentGem = grid.GetGridObject(x, y);
                if (!currentGem.IsGemGridCenter()) continue;

                int currentColor = currentGem.GetGemGrid().color;

                HandleMove(currentGem.GetNeighborLeft(), currentGem.GetNeighborRight(), currentColor, colorLimit);
                HandleMove(currentGem.GetNeighborTop(), currentGem.GetNeighborBottom(), currentColor, colorLimit);
            }
        }
    }

    private void HandleMove(GemGridPosition first, GemGridPosition second, int currentColor, int colorLimit)
    {
        if (first == null || second == null) return;

        if (first.ListNeighbor().Count > 0 || second.ListNeighbor().Count > 0)
        {
            second.GetGemGrid().color = currentColor;

            GemGridPosition selectedNeighbor = first.ListNeighbor().Count > 0
                ? first.ListNeighbor()[Random.Range(0, first.ListNeighbor().Count)]
                : second.ListNeighbor()[Random.Range(0, second.ListNeighbor().Count)];

            selectedNeighbor.GetGemGrid().color = currentColor;

           
            List<int> validColors = Enumerable.Range(0, colorLimit)
                .Where(c => c != currentColor && !first.WillCauseMatch3(c))
                .ToList();

            
            first.GetGemGrid().color = validColors.Count > 0 ? validColors[Random.Range(0, validColors.Count)]
                                                : (currentColor + 1) % colorLimit;

            
            List<int> validColorsSecond = Enumerable.Range(0, colorLimit)
                .Where(c => c != second.GetGemGrid().color && !second.WillCauseMatch3(c))
                .ToList();

            
            if (validColorsSecond.Count > 0)
            {
                second.GetGemGrid().color = validColorsSecond[Random.Range(0, validColorsSecond.Count)];
            }
            else
            {
                second.GetGemGrid().color = (second.GetGemGrid().color + 1) % colorLimit;
            }
        }
    }


    private int GenColor(GemGridPosition gemGridPosition, int colorLimit)
    {
        List<int> exceptColors = new List<int>();
        List<int> remainColors = new List<int>();

        for (int i = 0; i < colorLimit; i++)
        {
            if (gemGridPosition.WillCauseMatch3(i))
            {
                exceptColors.Add(i);
            }
        }

        for (int i = 0; i < colorLimit; i++)
        {
            if (!exceptColors.Contains(i))
            {
                remainColors.Add(i);
            }
        }

        if (remainColors.Count > 0)
        {
            return remainColors[Random.Range(0, remainColors.Count)];
        }

        int randColor;
        do
        {
            randColor = Random.Range(0, colorLimit);
        }
        while (exceptColors.Contains(randColor) && exceptColors.Count < colorLimit - 1);

        return randColor;
    }





    public Grid<GemGridPosition> GetGrid()
    {
        return grid;
    }

    public bool CanSwapGridPositions(int startX, int startY, int endX, int endY)
    {
        if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return false;

        if (startX == endX && startY == endY) return false;

        SwapGridPositions(startX, startY, endX, endY);

        bool hasLinkAfterSwap = HasMatch3Link(startX, startY) || HasMatch3Link(endX, endY);

        SwapGridPositions(startX, startY, endX, endY);

        return hasLinkAfterSwap;
    }

    public bool HasMatch3Link(int x, int y)
    {
        List<GemGridPosition> linkedGemGridPositionList = GetMatch3Links(x, y);
        return linkedGemGridPositionList != null && linkedGemGridPositionList.Count >= 3;
    }

    public List<GemGridPosition> GetMatch3Links(int x, int y)
    {
        GemSO gemSO = GetGemSO(x, y);

        if (gemSO == null) return null;

        int rightLinkAmount = 0;
        for (int i = 1; i < gridWidth; i++)
        {
            if (IsValidPosition(x + i, y))
            {
                GemSO nextGemSO = GetGemSO(x + i, y);
                if (nextGemSO == gemSO)
                {
                    rightLinkAmount++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        int leftLinkAmount = 0;
        for (int i = 1; i < gridWidth; i++)
        {
            if (IsValidPosition(x - i, y))
            {
                GemSO nextGemSO = GetGemSO(x - i, y);
                if (nextGemSO == gemSO)
                {
                    leftLinkAmount++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        int horizontalLinkAmount = 1 + leftLinkAmount + rightLinkAmount;

        if (horizontalLinkAmount >= 3)
        {
            List<GemGridPosition> linkedGemGridPositionList = new List<GemGridPosition>();
            int leftMostX = x - leftLinkAmount;
            for (int i = 0; i < horizontalLinkAmount; i++)
            {
                linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i, y));
            }
            return linkedGemGridPositionList;
        }


        int upLinkAmount = 0;
        for (int i = 1; i < gridHeight; i++)
        {
            if (IsValidPosition(x, y + i))
            {
                GemSO nextGemSO = GetGemSO(x, y + i);
                if (nextGemSO == gemSO)
                {
                    upLinkAmount++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        int downLinkAmount = 0;
        for (int i = 1; i < gridHeight; i++)
        {
            if (IsValidPosition(x, y - i))
            {
                GemSO nextGemSO = GetGemSO(x, y - i);
                if (nextGemSO == gemSO)
                {
                    downLinkAmount++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        int verticalLinkAmount = 1 + downLinkAmount + upLinkAmount;

        if (verticalLinkAmount >= 3)
        {
            List<GemGridPosition> linkedGemGridPositionList = new List<GemGridPosition>();
            int downMostY = y - downLinkAmount;
            for (int i = 0; i < verticalLinkAmount; i++)
            {
                linkedGemGridPositionList.Add(grid.GetGridObject(x, downMostY + i));
            }
            return linkedGemGridPositionList;
        }

        return null;
    }

    private GemSO GetGemSO(int x, int y)
    {
        if (!IsValidPosition(x, y)) return null;

        GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

        if (gemGridPosition.GetGemGrid() == null) return null;

        return gemGridPosition.GetGemGrid().GetGem();
    }

    public void SwapGridPositions(int startX, int startY, int endX, int endY)
    {
        if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return;

        if (startX == endX && startY == endY) return;

        GemGridPosition startGemGridPosition = grid.GetGridObject(startX, startY);
        GemGridPosition endGemGridPosition = grid.GetGridObject(endX, endY);

        GemGrid startGemGrid = startGemGridPosition.GetGemGrid();
        GemGrid endGemGrid = endGemGridPosition.GetGemGrid();

        startGemGrid.SetGemXY(endX, endY);
        endGemGrid.SetGemXY(startX, startY);

        startGemGridPosition.SetGemGrid(endGemGrid);
        endGemGridPosition.SetGemGrid(startGemGrid);
    }

    public void TestingGetAllMatch3Links(int startX, int startY, int endX, int endY)
    {
        SwapGridPositions(startX, startY, endX, endY);

        List<List<GemGridPosition>> allLinkedGemGridPositionList = GetAllMatch3Links();
        Debug.Log("allLinkedGemGridPositionList.Count: " + allLinkedGemGridPositionList.Count);

        foreach (List<GemGridPosition> linkedGemGridPositionList in allLinkedGemGridPositionList)
        {
            Debug.Log("linkedGemGridPositionList");
            foreach (GemGridPosition gemGridPosition in linkedGemGridPositionList)
            {
                Debug.Log(gemGridPosition.GetWorldPosition());
            }
        }

        SwapGridPositions(startX, startY, endX, endY);
    }

    private bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return false;
        }
        else if (grid.GetGridObject(x, y).IsBlock())
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool TryFindMatchesAndDestroyThem()
    {
        List<List<GemGridPosition>> allLinkedGemGridPositionList = GetAllMatch3Links();

        bool foundMatch = false;

        List<Vector2Int> explosionGridPositionList = new List<Vector2Int>();

        foreach (List<GemGridPosition> linkedGemGridPositionList in allLinkedGemGridPositionList)
        {
            foreach (GemGridPosition gemGridPosition in linkedGemGridPositionList)
            {
                TryDestroyGemGridPosition(gemGridPosition);
            }

            if (linkedGemGridPositionList.Count >= 4)
            {
                LevelManager.Instance.AddScore(200);

                GemGridPosition explosionOriginGemGridPosition = linkedGemGridPositionList[0];

                int explosionX = explosionOriginGemGridPosition.GetX();
                int explosionY = explosionOriginGemGridPosition.GetY();

                explosionGridPositionList.Add(new Vector2Int(explosionX - 1, explosionY - 1));
                explosionGridPositionList.Add(new Vector2Int(explosionX + 0, explosionY - 1));
                explosionGridPositionList.Add(new Vector2Int(explosionX + 1, explosionY - 1));
                explosionGridPositionList.Add(new Vector2Int(explosionX - 1, explosionY + 0));
                explosionGridPositionList.Add(new Vector2Int(explosionX + 1, explosionY + 0));
                explosionGridPositionList.Add(new Vector2Int(explosionX - 1, explosionY + 1));
                explosionGridPositionList.Add(new Vector2Int(explosionX + 0, explosionY + 1));
                explosionGridPositionList.Add(new Vector2Int(explosionX + 1, explosionY + 1));
            }

            foundMatch = true;
        }

        bool spawnExplosion = match4Explosions;

        if (spawnExplosion)
        {
            foreach (Vector2Int explosionGridPosition in explosionGridPositionList)
            {
                if (IsValidPosition(explosionGridPosition.x, explosionGridPosition.y))
                {
                    GemGridPosition gemGridPosition = grid.GetGridObject(explosionGridPosition.x, explosionGridPosition.y);
                    TryDestroyGemGridPosition(gemGridPosition);
                }
            }
        }

        return foundMatch;
    }

    private void TryDestroyGemGridPosition(GemGridPosition gemGridPosition)
    {
        if (gemGridPosition.HasGemGrid())
        {
            LevelManager.Instance.AddScore(100);

            gemGridPosition.DestroyGem();
            OnGemGridPositionDestroyed?.Invoke(gemGridPosition);
            gemGridPosition.ClearGemGrid();
        }
    }

    public List<List<GemGridPosition>> GetAllMatch3Links()
    {
        List<List<GemGridPosition>> allLinkedGemGridPositionList = new List<List<GemGridPosition>>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (HasMatch3Link(x, y))
                {
                    List<GemGridPosition> linkedGemGridPositionList = GetMatch3Links(x, y);

                    if (allLinkedGemGridPositionList.Count == 0)
                    {
                        allLinkedGemGridPositionList.Add(linkedGemGridPositionList);
                    }
                    else
                    {
                        bool uniqueNewLink = true;

                        foreach (List<GemGridPosition> tmpLinkedGemGridPositionList in allLinkedGemGridPositionList)
                        {
                            if (linkedGemGridPositionList.Count == tmpLinkedGemGridPositionList.Count)
                            {
                                bool allTheSame = true;
                                for (int i = 0; i < linkedGemGridPositionList.Count; i++)
                                {
                                    if (linkedGemGridPositionList[i] == tmpLinkedGemGridPositionList[i])
                                    {

                                    }
                                    else
                                    {
                                        allTheSame = false;
                                        break;
                                    }
                                }

                                if (allTheSame)
                                {
                                    uniqueNewLink = false;
                                }
                            }
                        }

                        if (uniqueNewLink)
                        {
                            allLinkedGemGridPositionList.Add(linkedGemGridPositionList);
                        }
                    }
                }
            }
        }

        return allLinkedGemGridPositionList;
    }

    public void FallGemsIntoEmptyPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

                if (gemGridPosition.IsBlock()) continue;

                if (!gemGridPosition.IsEmpty())
                {
                    for (int i = y - 1; i >= 0; i--)
                    {
                        GemGridPosition nextGemGridPosition = grid.GetGridObject(x, i);

                        if (nextGemGridPosition.IsBlock()) continue;

                        if (nextGemGridPosition.IsEmpty())
                        {
                            gemGridPosition.GetGemGrid().SetGemXY(x, i);
                            nextGemGridPosition.SetGemGrid(gemGridPosition.GetGemGrid());
                            gemGridPosition.ClearGemGrid();

                            gemGridPosition = nextGemGridPosition;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public void SpawnNewMissingGridPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

                if (gemGridPosition.IsBlock()) continue;

                if (gemGridPosition.IsEmpty())
                {
                    GemSO gem = levelData.gemList[Random.Range(0, levelData.gemList.Count)];
                    GemGrid gemGrid = new GemGrid(gem, x, y);

                    gemGridPosition.SetGemGrid(gemGrid);

                    OnNewGemGridSpawned?.Invoke(gemGrid, gemGridPosition);
                    SoundManager.Instance.PlaySoundsRandom(SoundManager.Instance.drop);
                }
            }
        }
    }

    public List<PossibleMove> GetAllPossibleMoves()
    {
        List<PossibleMove> allPossibleMovesList = new List<PossibleMove>();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                List<PossibleMove> testPossibleMoveList = new List<PossibleMove>();

                testPossibleMoveList.Add(new PossibleMove(x, y, x - 1, y + 0));
                testPossibleMoveList.Add(new PossibleMove(x, y, x + 1, y + 0));
                testPossibleMoveList.Add(new PossibleMove(x, y, x + 0, y + 1));
                testPossibleMoveList.Add(new PossibleMove(x, y, x + 0, y - 1));

                for (int i = 0; i < testPossibleMoveList.Count; i++)
                {
                    PossibleMove possibleMove = testPossibleMoveList[i];

                    bool skipPossibleMove = false;

                    for (int j = 0; j < allPossibleMovesList.Count; j++)
                    {
                        PossibleMove tmpPossibleMove = allPossibleMovesList[j];
                        if (tmpPossibleMove.startX == possibleMove.startX &&
                            tmpPossibleMove.startY == possibleMove.startY &&
                            tmpPossibleMove.endX == possibleMove.endX &&
                            tmpPossibleMove.endY == possibleMove.endY)
                        {
                            skipPossibleMove = true;
                            break;
                        }
                        if (tmpPossibleMove.startX == possibleMove.endX &&
                            tmpPossibleMove.startY == possibleMove.endY &&
                            tmpPossibleMove.endX == possibleMove.startX &&
                            tmpPossibleMove.endY == possibleMove.startY)
                        {
                            skipPossibleMove = true;
                            break;
                        }
                    }

                    if (skipPossibleMove)
                    {
                        continue;
                    }

                    SwapGridPositions(possibleMove.startX, possibleMove.startY, possibleMove.endX, possibleMove.endY);

                    List<List<GemGridPosition>> allLinkedGemGridPositionList = GetAllMatch3Links();

                    if (allLinkedGemGridPositionList.Count > 0)
                    {
                        possibleMove.allLinkedGemGridPositionList = allLinkedGemGridPositionList;
                        allPossibleMovesList.Add(possibleMove);
                    }

                    SwapGridPositions(possibleMove.startX, possibleMove.startY, possibleMove.endX, possibleMove.endY);
                }
            }
        }

        return allPossibleMovesList;
    }
}
