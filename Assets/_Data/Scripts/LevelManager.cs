using System;
using UnityEngine;

public enum GameState
{
    Menu,
    PrepareGame,
    SetupVisual,
    Busy,
    WaitingForUser,
    TryFindMatches,
    Shuffle,
    GameOver,
}

public class LevelManager : Singleton<LevelManager>
{
    private LevelSO currentLevelData;
    private int currentLevel = 1;

    [SerializeField] private FieldBoard fieldBoard;
    [SerializeField] private GameState state;

    private float busyTimer;
    private Action onBusyTimerElapsedAction;

    private int startDragX;
    private int startDragY;
    private Vector3 startDragMouseWorldPosition;

    private int score;
    private int moveCount;

    private int moveAmount;
    private int targetScore;

    private bool isShuffle = false;

    public Action<GameState> OnStateChanged;
    public Action OnMoveUsed;
    public Action OnScoreChanged;

    public Action OnOutOfMoves;
    public Action OnWin;

    public Action OnShuffle;



    private void Start()
    {
        SetState(GameState.Menu);
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Busy:
                busyTimer -= Time.deltaTime;
                if (busyTimer <= 0f)
                {
                    onBusyTimerElapsedAction?.Invoke();
                }
                break;

            case GameState.WaitingForUser:

                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWorldPosition = GetMouseWorldPosition();
                    fieldBoard.GetGrid().GetXY(mouseWorldPosition, out startDragX, out startDragY);

                    startDragMouseWorldPosition = mouseWorldPosition;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 mouseWorldPosition = GetMouseWorldPosition();
                    fieldBoard.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

                    if (x != startDragX)
                    {
                        y = startDragY;

                        if (x < startDragX)
                        {
                            x = startDragX - 1;
                        }
                        else
                        {
                            x = startDragX + 1;
                        }
                    }
                    else
                    {
                        x = startDragX;

                        if (y < startDragY)
                        {
                            y = startDragY - 1;
                        }
                        else
                        {
                            y = startDragY + 1;
                        }
                    }

                    SwapGridPositions(startDragX, startDragY, x, y);
                }
                break;

            case GameState.TryFindMatches:
                if (fieldBoard.TryFindMatchesAndDestroyThem())
                {
                    SetBusyState(.3f, () =>
                    {
                        fieldBoard.FallGemsIntoEmptyPositions();

                        SetBusyState(.3f, () =>
                        {
                            fieldBoard.SpawnNewMissingGridPositions();

                            SetBusyState(.5f, () => SetState(GameState.TryFindMatches));
                        });
                    });
                }
                else
                {
                    TrySetStateWaitingForUser();
                }

                break;
        }
    }

    private void TrySetStateWaitingForUser()
    {
        if (WinLevel())
        {
            OnWin?.Invoke();
            SetBusyState(1f, () =>
            {
                currentLevel++;
                LoadLevel();
                SetState(GameState.PrepareGame);

            });
        }
        else if (TryIsGameOver())
        {
            Debug.Log("Game Over!");
            SetState(GameState.GameOver);
        }
        else if (fieldBoard.GetAllPossibleMoves().Count == 0)
        {
            OnShuffle?.Invoke();
            isShuffle = true;

            SetBusyState(.5f, () =>
            {
                SetState(GameState.Shuffle);

            });
        }
        else
        {
            SetState(GameState.WaitingForUser);
        }
    }

    public void SwapGridPositions(int startX, int startY, int endX, int endY)
    {
        fieldBoard.SwapGridPositions(startX, startY, endX, endY);
        bool hasLinkAfterSwap = fieldBoard.HasMatch3Link(startX, startY) || fieldBoard.HasMatch3Link(endX, endY);

        SetBusyState(.6f, () =>
        {

            if (hasLinkAfterSwap)
            {
                UseMove();
            }
            else
            {
                fieldBoard.SwapGridPositions(startX, startY, endX, endY);
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.wrong_match);
            }
            SetState(GameState.TryFindMatches);
        });
    }

    public void UseMove()
    {
        moveCount--;
        OnMoveUsed?.Invoke();
    }

    public void AddScore(int value)
    {
        score += value;
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.click);
        OnScoreChanged?.Invoke();
    }

    private bool TryIsGameOver()
    {
        if (!HasMoveAvailable())
        {
            OnOutOfMoves?.Invoke();
            return true;
        }

        return false;
    }

    private bool WinLevel()
    {
        return score >= targetScore;
    }

    public void SetState(GameState gameState)
    {
        this.state = gameState;
        OnStateChanged?.Invoke(this.state);
    }

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction)
    {
        SetState(GameState.Busy);
        this.busyTimer = busyTimer;
        this.onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }

    public void StartGame()
    {
        LoadLevel();

        SetState(GameState.PrepareGame);
    }

    public void SetupVisual() => SetState(GameState.SetupVisual);

    public void SetupVisualCompleted()
    {
        SetBusyState(1f, () => SetState(GameState.TryFindMatches));

        isShuffle = false;
    }

    private void LoadLevel()
    {
        var level = ScriptableLevelManager.LoadLevel(currentLevel);

        if (level != null)
        {
            currentLevelData = level;
            Debug.Log("Loaded");
        }

        score = 0;
        moveCount = currentLevelData.moveAmount;
        targetScore = currentLevelData.targetScore;
    }

    public FieldBoard GetFieldBoard()
    {
        return fieldBoard;
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public LevelSO GetCurrentLevelData()
    {
        return currentLevelData;
    }

    public int GetTargetScore()
    {
        return currentLevelData.targetScore;
    }

    public GameState GetState()
    {
        return state;
    }

    public int GetScore()
    {
        return score;
    }

    public bool HasMoveAvailable()
    {
        return moveCount > 0;
    }

    public int GetMoveCount()
    {
        return moveCount;
    }

    public int GetUsedMoveCount()
    {
        return moveAmount - moveCount;
    }

    public bool IsShuffle()
    {
        return isShuffle;
    }
}
