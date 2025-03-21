using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3Bot : MonoBehaviour
{
    private FieldBoard fieldBoard;
  
    private void Awake()
    {
        LevelManager.Instance.OnStateChanged += LevelManager_OnStateChanged;
        fieldBoard = LevelManager.Instance.GetFieldBoard();
    }

    private void LevelManager_OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingForUser:
                BotDoMove();
                break;
        }
    }

    private void BotDoMove()
    {
        List<PossibleMove> possibleMoveList = fieldBoard.GetAllPossibleMoves();

        PossibleMove bestPossibleMove = GetBestPossibleMove(possibleMoveList);

        if (bestPossibleMove == null)
        {
            Debug.Log("Bot cannot find a possible move!");
        }
        else
        {
            LevelManager.Instance.SwapGridPositions(bestPossibleMove.startX, bestPossibleMove.startY, bestPossibleMove.endX, bestPossibleMove.endY);
        }
    }

    private PossibleMove GetBestPossibleMove(List<PossibleMove> possibleMoveList)
    {
        PossibleMove bestPossibleMove = null;

        foreach (PossibleMove possibleMove in possibleMoveList)
        {
            if (bestPossibleMove == null)
            {
                bestPossibleMove = possibleMove;
            }
            else
            {
                if (possibleMove.GetTotalMatchAmount() > bestPossibleMove.GetTotalMatchAmount())
                {
                    bestPossibleMove = possibleMove;
                }
            }
        }
        return bestPossibleMove;
    }
}

