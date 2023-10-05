
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

public class MenuSphere : MonoBehaviourPunCallbacks
{
    // Public Variables //

    public CanvasGroup msPanel;
    public float animDuration = 0.1f;
    public List<CanvasGroup> infoPanels = new List<CanvasGroup>();
    public List<CanvasGroup> infoTextBoxes = new List<CanvasGroup>();


    // Private Variables //

    private bool menuOpen = false;
    private TextMeshProUGUI msPanelText;


    private void Start()
    {
        msPanelText = msPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnHoverEntered()
    {
        if (!menuOpen)
        {
            msPanelText.text = "Show Information";
            StartCoroutine(FadeCanvasGroup(msPanel, 0f, 1f));
            photonView.RPC(nameof(UpdateMSPanel), RpcTarget.Others, true, 1);
        }
        else if (menuOpen) 
        {
            msPanelText.text = "Exit";
            StartCoroutine(FadeCanvasGroup(msPanel, 0f, 1f));
            photonView.RPC(nameof(UpdateMSPanel), RpcTarget.Others, true, 2);
        }
    }

    public void OnHoverExited()
    {
        StartCoroutine(FadeCanvasGroup(msPanel, 1f, 0f));
        photonView.RPC(nameof(UpdateMSPanel), RpcTarget.Others, false, 0);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, bool enableInteraction = false)
    {
        float t = 0f;
        while (t < animDuration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        cG.alpha = endAlpha;

        if (enableInteraction)
        {
            cG.interactable = true;
            cG.blocksRaycasts = true;
        }
        else
        {
            cG.interactable = false;
            cG.blocksRaycasts = false;
        }
    }

    public void OnSelectEntered()
    {
        if (!menuOpen)
        {
            menuOpen = true;

            // Show information panels
            StartCoroutine(FadeInformationPanels(0f, 1f, true));
            photonView.RPC(nameof(UpdateInfoPanels), RpcTarget.Others, true);
        }
        else
        {
            menuOpen = false;

            // Hide information panels
            StartCoroutine(FadeInformationPanels(1f, 0f, false));
            photonView.RPC(nameof(UpdateInfoPanels), RpcTarget.Others, false);
        }
    }

    private IEnumerator FadeInformationPanels(float startAlpha, float endAlpha, bool enableInteraction)
    {
        for (int i = 0; i < infoPanels.Count; i++)
        {
            StartCoroutine(FadeCanvasGroup(infoPanels[i], startAlpha, endAlpha, enableInteraction));

            yield return new WaitForSeconds(0.05f);
        }
    }

    [PunRPC]
    void UpdateMSPanel(bool visibility, int state)
    {
        if (visibility)
        {
            msPanel.alpha = 1f;
            msPanelText.text = state == 1 ? "Show Information" : "Exit";
        }
        else
        {
            msPanel.alpha = 0f;
        }
    }

    [PunRPC]
    void UpdateInfoPanels(bool visibility)
    {
        if (visibility)
        {
            for (int i = 0; i < infoPanels.Count; i++)
            {
                infoPanels[i].alpha = 1f;
                infoPanels[i].interactable = true;
                infoPanels[i].blocksRaycasts = true;
            }
        }
        else
        {
            for (int i = 0; i < infoPanels.Count; i++)
            {
                infoPanels[i].alpha = 0f;
                infoPanels[i].interactable = false;
                infoPanels[i].blocksRaycasts = false;
            }
        }
    }

    [PunRPC]
    void UpdateInfoTextBox(string name, bool visibility, string text)
    {
        CanvasGroup canvasGroup = null;
        TextMeshProUGUI displayText = null;
        for (int i = 0; i < infoTextBoxes.Count; i++)
        {
            if (name == infoTextBoxes[i].name)
            {
                canvasGroup = infoTextBoxes[i];
                displayText = canvasGroup.transform.Find("Viewport/Content/Text").GetComponent<TextMeshProUGUI>();
                break;
            }
        }

        if (canvasGroup != null)
        {
            if (visibility)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                displayText.text = text;
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}