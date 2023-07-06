using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionGuide : MonoBehaviour
{
    // Public Variables

    public GameObject guideCanvas;
    public float animDuration = 0.1f;
    [HideInInspector]
    public bool isMenuOpen = false;


    // Private Variables 

    private bool isAnimating = false;
    private CanvasGroup cG;

    private void Start()
    {
        cG = guideCanvas.GetComponent<CanvasGroup>();
    }

    public void HideGuide()
    {
        if (cG.alpha != 0f)
            StartCoroutine(AnimateCanvasAlpha(cG.alpha, 0f, true));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            // Show Interaction Guide
            if (!isAnimating && !isMenuOpen)
            {
                guideCanvas.SetActive(true);
                StartCoroutine(AnimateCanvasAlpha(0f, 1f, false));
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
                StartCoroutine(AnimateCanvasAlpha(1f, 0f, true));
            }
        }
    }

    private IEnumerator AnimateCanvasAlpha(float startAlpha, float endAlpha, bool disableAfter)
    {
        float t = 0f;
        while (t < animDuration)
        {
            isAnimating = true;

            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        isAnimating = false;

        if(disableAfter)
            guideCanvas.SetActive(false);
    }
}
