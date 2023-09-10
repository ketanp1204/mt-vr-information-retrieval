
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class MenuSphere : XRSimpleInteractable
{
    // Public Variables //

    public CanvasGroup msPanel;
    public float animDuration = 0.1f;
    public List<CanvasGroup> infoPanels = new List<CanvasGroup>();


    // Private Variables //

    private bool menuOpen = false;



    private void Start()
    {
        
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        if (!menuOpen)
        {
            StartCoroutine(FadeCanvasGroup(msPanel, 0f, 1f, 3f));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, float restoreAfterDelay)
    {
        float t = 0f;
        while (t < animDuration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        if (restoreAfterDelay > 0f)
        {
            yield return new WaitForSeconds(restoreAfterDelay);

            if(cG.alpha == 1f)
            {
                t = 0f;
                while (t < animDuration)
                {
                    cG.alpha = Mathf.Lerp(endAlpha, startAlpha, t / animDuration);

                    t += Time.deltaTime;
                    yield return null;
                }

                cG.alpha = startAlpha;
            }
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (!menuOpen)
        {
            menuOpen = true;

            StartCoroutine(FadeInformationPanels(0f, 1f));
        }
        else
        {
            menuOpen = false;

            StartCoroutine(FadeInformationPanels(1f, 0f));
        }
    }

    private IEnumerator FadeInformationPanels(float startAlpha, float endAlpha)
    {
        for (int i = 0; i < infoPanels.Count; i++)
        {
            StartCoroutine(FadeCanvasGroup(infoPanels[i], startAlpha, endAlpha, 0f));

            yield return new WaitForSeconds(0.05f);
        }
    }
}
