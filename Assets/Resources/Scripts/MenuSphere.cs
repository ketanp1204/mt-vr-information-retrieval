
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
using UnityEngine.UIElements;
using UnityEngine.Video;

public class MenuSphere : MonoBehaviourPunCallbacks
{
    // Public Variables //

    [Header("Exhibit Information")]
    public ExhibitInformation exhibitInfo;

    [Space(20)]
    [Header("UI References")]
    public CanvasGroup msPanel;
    public GameObject videoPlayerBox;
    public AudioSource audioSource;    
    public List<CanvasGroup> infoPanels = new List<CanvasGroup>();
    public List<CanvasGroup> infoTextBoxes = new List<CanvasGroup>();

    [Space(20)]
    [Header("Prefabs")]
    public GameObject imagePrefab2D;
    public GameObject videoPrefab2D;


    // Private Variables //

    private bool menuOpen = false;
    private TextMeshProUGUI msPanelText;
    private float animDuration = 0.1f;



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
        Transform videosContainer = GetChildWithName(infoPanels[1].gameObject, "VideosContainer").transform;

        for (int i = 0; i < exhibitInfo.detailInfoVideos.Length; i++)
        {
            GameObject video = Instantiate(videoPrefab2D, videosContainer);
            video.GetComponent<VideoPrefab2D>().SetData(exhibitInfo.detailInfoVideos[i].videoClip,
                                                        exhibitInfo.detailInfoVideos[i].videoClipThumbnail,
                                                        exhibitInfo.detailInfoVideos[i].videoClipText.text,
                                                        infoTextBoxes[0].GetComponent<TextBox>(),
                                                        videoPlayerBox,
                                                        videoPlayerBox.GetComponentInChildren<VideoPlayer>());
        }


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
        Transform relatedItemsContainer = GetChildWithName(infoPanels[3].gameObject, "RelatedItemsContainer").transform;

        for (int i = 0; i < exhibitInfo.detailInfoRelatedItems.Length; i++)
        {
            // Item of type image
            if (exhibitInfo.detailInfoRelatedItems[i].imageInfo.image != null)
            {
                GameObject image = Instantiate(imagePrefab2D, relatedItemsContainer);
                image.GetComponent<ImagePrefab2D>().SetData(exhibitInfo.detailInfoRelatedItems[i].imageInfo.image,
                                                        exhibitInfo.detailInfoRelatedItems[i].imageInfo.imageText.text,
                                                        infoTextBoxes[2].GetComponent<TextBox>());
            }

            // Item of type video
            if (exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClip != null)
            {
                GameObject image = Instantiate(imagePrefab2D, relatedItemsContainer);
                image.GetComponent<ImagePrefab2D>().SetData(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipThumbnail,
                                                        exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipText.text,
                                                        infoTextBoxes[2].GetComponent<TextBox>());
            }

            // Item of type model
            if (exhibitInfo.detailInfoRelatedItems[i].modelInfo.model != null)
            {
                GameObject image = Instantiate(imagePrefab2D, relatedItemsContainer);
                image.GetComponent<ImagePrefab2D>().SetData(exhibitInfo.detailInfoRelatedItems[i].modelInfo.model2DPreviewSprite,
                                                        exhibitInfo.detailInfoRelatedItems[i].modelInfo.modelText.text,
                                                        infoTextBoxes[2].GetComponent<TextBox>());
            }
        }



        // Description
        TextMeshProUGUI descTextArea = GetChildWithName(infoPanels[4].gameObject, "DescriptionTextArea").GetComponent<TextMeshProUGUI>();

        descTextArea.text = exhibitInfo.basicInfoText + "\n" + exhibitInfo.detailInfoText;

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

            // Hide info text boxes
            for (int i = 0; i < infoTextBoxes.Count;  i++)
            {
                infoTextBoxes[i].alpha = 0f;
                infoTextBoxes[i].interactable = false;
                infoTextBoxes[i].blocksRaycasts = false;
            }

            // Hide video player
            videoPlayerBox.SetActive(false);

            // Reset audio player
            audioSource.Stop();
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