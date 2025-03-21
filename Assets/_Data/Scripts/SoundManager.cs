using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource audioSource;

    public AudioClip click;
    public AudioClip[] drop;
    public AudioClip[] gameOver;
    public AudioClip[] complete;
    public AudioClip noMatch;
    public AudioClip wrong_match;

    public void PlayOneShot(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void PlaySoundsRandom(AudioClip[] clip)
    {
        if (clip.Length > 0)
        {
            PlayOneShot(clip[Random.Range(0, clip.Length)]);
        }
            
    }
}
