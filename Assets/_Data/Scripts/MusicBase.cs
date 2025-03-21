using UnityEngine;

public class MusicBase : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menu;
    public AudioClip[] music;

    private void Awake()
    {
        LevelManager.Instance.OnStateChanged += LevelManager_OnStateChanged;
    }

    private void LevelManager_OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                audioSource.clip = menu;
                audioSource.loop = true;
                audioSource.Play();
                break;

            case GameState.PrepareGame:
                audioSource.clip = music[Random.Range(0, music.Length)];
                audioSource.loop = true;
                audioSource.Play();
                break;
        }
    }
}
