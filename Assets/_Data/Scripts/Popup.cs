using System.Collections;
using UnityEngine;
[RequireComponent(typeof(CanvasGroup))]
public class Popup : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void ShowPopup()
    {
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        yield return ChangeCoroutine(0, 1);
        yield return new WaitForSeconds(1f);
        yield return ChangeCoroutine(1, 0);

        gameObject.SetActive(false);
    }

    private IEnumerator ChangeCoroutine(int startAlpha, int targetAlpha)
    {

        float duration = .2f;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            elaspedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

    }
}
