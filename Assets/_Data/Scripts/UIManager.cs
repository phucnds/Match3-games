using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject panel;

    [SerializeField] private Button btnPlay;

    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;

    [SerializeField] private Popup popupWin;
    [SerializeField] private Popup popupLose;
    [SerializeField] private Popup popupShuffle;

    private List<GameObject> panels = new List<GameObject>();

    private void Awake()
    {
        panels.AddRange(new GameObject[] {
            menu,
            panel,
        });

        LevelManager.Instance.OnStateChanged += LevelManager_OnStateChanged;
        LevelManager.Instance.OnMoveUsed += Match3_OnMoveUsed;
        LevelManager.Instance.OnScoreChanged += Match3_OnScoreChanged;

        LevelManager.Instance.OnOutOfMoves += Match3_OnOutOfMoves;
        LevelManager.Instance.OnWin += Match3_OnWin;
        LevelManager.Instance.OnShuffle += Match3_OnShuffle;

        btnPlay.onClick.AddListener(PlayGame);
    }

    private void LevelManager_OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                ShowPanel(menu);
                break;

            case GameState.PrepareGame:
                ShowPanel(panel);
                break;

            case GameState.SetupVisual:
                targetScoreText.text = LevelManager.Instance.GetTargetScore().ToString();
                UpdateText();
                break;
        }
    }

    private void ShowPanel(GameObject panel)
    {
        foreach (GameObject item in panels)
        {
            item.SetActive(item == panel);
        }
    }

    private void Match3_OnWin()
    {
        Debug.Log("WIn");
        popupWin.gameObject.SetActive(true);
        popupWin.ShowPopup();
        SoundManager.Instance.PlaySoundsRandom(SoundManager.Instance.complete);
    }

    private void Match3_OnOutOfMoves()
    {
        Debug.Log("Lose");
        popupLose.gameObject.SetActive(true);
        popupLose.ShowPopup();
        SoundManager.Instance.PlaySoundsRandom(SoundManager.Instance.gameOver);
    }

    private void Match3_OnShuffle()
    {
        Debug.Log("Shuffle");
        popupShuffle.gameObject.SetActive(true);
        popupShuffle.ShowPopup();
        SoundManager.Instance.PlayOneShot(SoundManager.Instance.noMatch);
    }

    private void Match3_OnScoreChanged()
    {
        UpdateText();
    }

    private void Match3_OnMoveUsed()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        movesText.text = LevelManager.Instance.GetMoveCount().ToString();
        scoreText.text = LevelManager.Instance.GetScore().ToString();
    }

    private void PlayGame()
    {
        LevelManager.Instance.StartGame();
    }
}
