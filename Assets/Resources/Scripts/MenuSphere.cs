
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
using Photon.Realtime;
using UnityEngine.UI;

public class MenuSphere : MonoBehaviourPunCallbacks
{
    // Public Variables //

    [Header("Exhibit Information")]
    public ExhibitInformation exhibitInfo;

    [Space(20)]
    [Header("UI Properties")]
    public CanvasGroup msPanel;
    public float animDuration = 0.1f;
    public List<CanvasGroup> infoPanels = new List<CanvasGroup>();
    public List<CanvasGroup> infoTextBoxes = new List<CanvasGroup>();
    public List<Scrollbar> scrollbars = new List<Scrollbar>();

    [Space(20)]
    [Header("Prefabs")]
    public GameObject imagePrefab2D;


    // Private Variables //

    private bool menuOpen = false;
    private TextMeshProUGUI msPanelText;



    private void Start()
    {
        msPanelText = msPanel.GetComponentInChildren<TextMeshProUGUI>();
        SetInformationPanels();
    }

    private void SetInformationPanels()
    {
        // Audio Guide
        infoPanels[0].transform.GetComponentInChildren<AudioSource>().clip = exhibitInfo.menuSphereAudio;

        // Videos



        // Images
        Transform imagesContainer = GetChildWithName(infoPanels[2].gameObject, "ImagesContainer").transform;

        for (int i = 0; i < exhibitInfo.basicInfoImages.Length; i++)
        {
            GameObject image = Instantiate(imagePrefab2D, imagesContainer);
            image.GetComponent<ImagePrefab2D>().SetData(exhibitInfo.basicInfoImages[i].image,
                                                        exhibitInfo.basicInfoImages[i].imageText.text,
                                                        infoTextBoxes[1].GetComponent<TextBox>());
        }
        for (int i = 0; i < exhibitInfo.detailInfoImages.Length; i++)
        {
            GameObject image = Instantiate(imagePrefab2D, imagesContainer);
            image.GetComponent<ImagePrefab2D>().SetData(exhibitInfo.detailInfoImages[i].image,
                                                        exhibitInfo.detailInfoImages[i].imageText.text,
                                                        infoTextBoxes[1].GetComponent<TextBox>());
        }



        // Related Items



        // Description



    }

    private void SetScrollbarsValueUpdate()
    {
        for (int i = 0; i < scrollbars.Count; i++)
        {
            scrollbars[i].onValueChanged.AddListener((float val) => UpdateScrollBarValue(i));
        }
    }

    public void UpdateScrollBarValue(int index)
    {
        int scrollBarIndex = index;
        
        Debug.Log(scrollBarIndex);
        photonView.RPC(nameof(UpdateScrollBarValueRPC), RpcTarget.Others, scrollBarIndex, scrollbars[scrollBarIndex].value);
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
            photonView.RPC(nameof(UpdateMenuOpenBool), RpcTarget.Others, true);

            // Show information panels
            StartCoroutine(FadeInformationPanels(0f, 1f, true));
            photonView.RPC(nameof(UpdateInfoPanels), RpcTarget.Others, true);
        }
        else
        {
            menuOpen = false;
            photonView.RPC(nameof(UpdateMenuOpenBool), RpcTarget.Others, false);

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
    void UpdateMenuOpenBool(bool status)
    {
        menuOpen = status;
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

    [PunRPC]
    void UpdateScrollBarValueRPC(int scrollbarIndex, float value)
    {
        scrollbars[scrollbarIndex].value = value;
    }

    private Transform GetChildWithName(GameObject gO, string childName)
    {
        Transform child = null;
        foreach (Transform t in gO.GetComponentsInChildren<Transform>())
        {
            if (t.name == childName)
            {
                child = t;
                break;
            }
        }
        return child;
    }

    /*
    // Late join stuff

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetInformationPanels), newPlayer);
        }
    }
    */
}