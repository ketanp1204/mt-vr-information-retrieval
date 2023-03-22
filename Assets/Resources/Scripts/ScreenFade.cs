using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFade : MonoBehaviour
{
    /* Public Variables */
    public bool fadeOnStart = true;
    public float fadeDuration = 1.0f;
    public Color fadeColor;

    /* Private Variables */
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        if (fadeOnStart)
            FadeIn();
    }

    public void FadeIn()
    {
        Fade(1f, 0f, true);
    }

    public void FadeOut()
    {
        Fade(0f, 1f, false);
    }

    private void Fade(float alphaIn, float alphaOut, bool disableRenderer)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut, disableRenderer));
    }

    private IEnumerator FadeRoutine(float alphaIn, float alphaOut, bool disableRenderer)
    {
        Color c;

        rend.enabled = true;

        float timer = 0f;
        while (timer <= fadeDuration)
        {
            c = fadeColor;
            c.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);

            rend.material.SetColor("_Color", c);

            timer += Time.deltaTime;
            yield return null;
        }

        c = fadeColor;
        c.a = alphaOut;
        rend.material.SetColor("_Color", c);

        if (disableRenderer)
            rend.enabled = false;
    }
}
