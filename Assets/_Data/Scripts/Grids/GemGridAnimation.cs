using UnityEngine;

public class GemGridAnimation : MonoBehaviour
{
    [SerializeField] private Animation anim;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private SpriteRenderer sprite;
    
    public void PlayEffect()
    {
        anim.Play();
        particle.Play();
    }

    public void Release()
    {
        ObjectPooler.Instance.PutBack(gameObject);
    }

    public void ResetState()
    {
        sprite.transform.localScale = Vector3.one;
        sprite.color = Color.white;
    }
}
