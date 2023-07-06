using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionGuide : MonoBehaviour
{
    // Public Variables

    public CanvasGroup guideCanvasGroup;
    public float animDuration = 0.1f;
    [HideInInspector]
    public bool isMenuOpen = false;


    // Private Variables 

    private bool isAnimating = false;


    public void HideGuide()
    {
        if (guideCanvasGroup.alpha != 0f)
            StartCoroutine(AnimateCanvasAlpha(guideCanvasGroup.alpha, 0f));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Show Interaction Guide
            if (!isAnimating && !isMenuOpen)
            {
                StartCoroutine(AnimateCanvasAlpha(0f, 1f));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Hide Interaction Guide
            if (!isAnimating && !isMenuOpen)
            {
                StartCoroutine(AnimateCanvasAlpha(1f, 0f));
            }
        }
    }

    private IEnumerator AnimateCanvasAlpha(float startAlpha, float endAlpha)
    {
        float t = 0f;
        while (t < animDuration)
        {
            isAnimating = true;

            guideCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        guideCanvasGroup.alpha = endAlpha;

        isAnimating = false;
    }
}
