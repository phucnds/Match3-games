using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LevelVisual : MonoBehaviour
{
    public Action OnStateChanged;

    public enum State
    {
        Busy,
        WaitingForUser,
        TryFindMatches,
        GameOver,
    }

    [SerializeField] private Transform cameraTransform;

    private FieldBoard fieldBoard;

    private Grid<GemGridPosition> grid;
    private Dictionary<GemGrid, GemGridVisual> gemGridDictionary = new Dictionary<GemGrid, GemGridVisual>();

    private bool isSetup = false;

    private List<GameObject> lstBackgroundGO = new List<GameObject>();

    private void Awake()
    {
        LevelManager.Instance.OnStateChanged += LevelManager_OnStateChanged;

        fieldBoard = LevelManager.Instance.GetFieldBoard();

        fieldBoard.OnGemGridPositionDestroyed += Match3_OnGemGridPositionDestroyed;
        fieldBoard.OnNewGemGridSpawned += Match3_OnNewGemGridSpawned;
    }

    private void LevelManager_OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.SetupVisual:
                Setupvisual();
                break;
        }
    }

    private void Setupvisual()
    {
        isSetup = false;

        grid = fieldBoard.GetGrid();

        SetupGem();

        if (!LevelManager.Instance.IsShuffle()) SetupBackground();

        float cameraYOffset = 1f;
        cameraTransform.position = new Vector3(grid.GetWidth() * .5f, grid.GetHeight() * .5f + cameraYOffset, cameraTransform.position.z);
        LevelManager.Instance.SetupVisualCompleted();

        isSetup = true;
    }

    private void SetupGem()
    {
        if (gemGridDictionary.Keys.Count > 0)
        {
            ClearVisual();
        }

        gemGridDictionary = new Dictionary<GemGrid, GemGridVisual>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
                Vector3 position = grid.GetWorldPosition(x, y);


                if (gemGridPosition.IsBlock()) continue;

                GemGrid gemGrid = gemGridPosition.GetGemGrid();

                Transform gemGridVisualTransform = ObjectPooler.Instance.GetPooledObject("GemGridVisual", this).transform;
                position = new Vector3(position.x, grid.GetHeight());
                gemGridVisualTransform.position = position;

                gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = gemGrid.GetGem().sprite;
                gemGridVisualTransform.GetComponent<GemGridAnimation>().ResetState();

                GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, gemGrid);

                gemGridDictionary[gemGrid] = gemGridVisual;

            }
        }
    }

    private void SetupBackground()
    {
        if (lstBackgroundGO.Count > 0)
        {
            foreach (GameObject go in lstBackgroundGO)
            {
                ObjectPooler.Instance.PutBack(go);
            }

            lstBackgroundGO.Clear();
        }

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {



                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
                Vector3 position = grid.GetWorldPosition(x, y);

                Transform backgroundVisualTransform = ObjectPooler.Instance.GetPooledObject("BackgroundGridVisual", this).transform;
                backgroundVisualTransform.position = position;

                Texture2D texture = gemGridPosition.GetTexture();

                backgroundVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite =
                Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 200);

                lstBackgroundGO.Add(backgroundVisualTransform.gameObject);
            }
        }
    }

    private void Match3_OnGemGridPositionDestroyed(GemGridPosition gemGridPosition)
    {
        if (gemGridPosition != null && gemGridPosition.GetGemGrid() != null)
        {
            gemGridDictionary.Remove(gemGridPosition.GetGemGrid());
        }
    }

    private void Match3_OnNewGemGridSpawned(GemGrid gemGrid, GemGridPosition gemGridPosition)
    {
        Vector3 position = gemGridPosition.GetWorldPosition();
        position = new Vector3(position.x, grid.GetHeight());

        Transform gemGridVisualTransform = ObjectPooler.Instance.GetPooledObject("GemGridVisual", this).transform;
        gemGridVisualTransform.position = position;
        gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = gemGrid.GetGem().sprite;
        gemGridVisualTransform.GetComponent<GemGridAnimation>().ResetState();

        GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, gemGrid);

        gemGridDictionary[gemGrid] = gemGridVisual;
    }

    private void Update()
    {
        if (!isSetup) return;

        UpdateVisual();

        return;
    }

    private void UpdateVisual()
    {
        foreach (GemGrid gemGrid in gemGridDictionary.Keys)
        {
            gemGridDictionary[gemGrid].Update();
        }
    }

    private void ClearVisual()
    {
        foreach (GemGrid gemGrid in gemGridDictionary.Keys)
        {
            gemGrid.Destroy(false);
        }
    }
}
