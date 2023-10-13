using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using System;

public class ImagePrefab : MonoBehaviourPunCallbacks
{

    // Public Variables //

    public Image imageComp;
    public CanvasGroup textPanelCG;
    public TextMeshProUGUI textField;
    public Tooltip showTextTooltip;
    public InputActionReference showTextInputAction;


    // Private Variables

    private bool isHovering = false;
    private bool enableTextTooltip = false;
    private bool isTextVisible = false;
    private bool isDataSet = false;
    private TooltipHandler tooltipHandler;
    private string showTextString = "Show Info";
    private string hideTextString = "Hide Info";
    private string gOName = "";
    public ExhibitInformation exhibitInfo = null;
    private string exhibitNameString = "";
    private int exhibitInfoItemIndex = 0;
    private int exhibitInfoContentType = 0;


    
    public void SetImage(Sprite image)
    {
        // Set and resize image on the child
        imageComp.sprite = image;
        float aspectRatio = image.rect.width / image.rect.height;
        var fitter = imageComp.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = aspectRatio;

        // Resize box collider
        BoxCollider c = imageComp.GetComponent<BoxCollider>();
        RectTransform rt = imageComp.GetComponent<RectTransform>();
        c.size = new Vector3(rt.rect.width, rt.rect.height, c.size.z);

        isDataSet = true;
    }

    public void SetText(string text)
    {
        textField.text = text;
        enableTextTooltip = true;

        isDataSet = true;
    }

    public void SetInfoFromExhibitInfo(string exhibitName, int index, int contentType)
    {
        photonView.RPC(nameof(SetInfoFromExhibitInfoRPC), RpcTarget.All, exhibitName, index, contentType);
    }

    private void SetExhibitInfo(string exhibitName, int index, int contentType)
    {
        // Get exhibit information object
        ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
        for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
        {
            if (exhibitInfoRefs.exhibitInfos[i].exhibitName == exhibitName)
            {
                exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
            }
        }

        exhibitNameString = exhibitName;
        exhibitInfoItemIndex = index;
        exhibitInfoContentType = contentType;
    }

    [PunRPC]
    void SetInfoFromExhibitInfoRPC(string exhibitName, int index, int contentType)
    {
        SetExhibitInfo(exhibitName, index, contentType);

        if (contentType == 0)               // Basic Info Images
        {
            // Set exhibit info values
            SetImage(exhibitInfo.basicInfoImages[index].image);
            SetText(exhibitInfo.basicInfoImages[index].imageText.text);

            // Update GameObject name
            gOName = "BImages" + index.ToString();
            gameObject.name = gOName;
        }
        else if (contentType == 1)          // Detail Info Images
        {
            // Set exhibit info values
            SetImage(exhibitInfo.detailInfoImages[index].image);
            SetText(exhibitInfo.detailInfoImages[index].imageText.text);

            // Update GameObject name
            gOName = "DVImages" + index.ToString();
            gameObject.name = gOName;
        }
        else if (contentType == 2)          // Related Items Image
        {
            // Set exhibit info values
            SetImage(exhibitInfo.detailInfoRelatedItems[index].imageInfo.image);
            SetText(exhibitInfo.detailInfoRelatedItems[index].imageInfo.imageText.text);

            // Update GameObject name
            gOName = "DVRelatedItems" + index.ToString();
            gameObject.name = gOName;
        }
    }

    public void ShowHideText(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            if (!isTextVisible)
            {
                // Show Text Panel
                StartCoroutine(FadeCanvasGroup(textPanelCG, 0f, 1f, 0.1f, enableInteraction: true));
                isTextVisible = true;

                // Change tooltip string
                showTextTooltip.tooltipText = hideTextString;
            }
            else
            {
                // Hide Text Panel
                StartCoroutine(FadeCanvasGroup(textPanelCG, 1f, 0f, 0.1f, enableInteraction: false));
                isTextVisible = false;

                // Change tooltip string
                showTextTooltip.tooltipText = showTextString;
            }

            photonView.RPC(nameof(UpdateTextPanel), RpcTarget.Others, isTextVisible);
        }
    }

    [PunRPC]
    void UpdateTextPanel(bool visibility)
    {
        if (visibility)
        {
            // Show Text Panel
            StartCoroutine(FadeCanvasGroup(textPanelCG, 0f, 1f, 0.1f, enableInteraction: true));
            isTextVisible = true;

            // Change tooltip string
            showTextTooltip.tooltipText = hideTextString;
        }
        else
        {
            // Hide Text Panel
            StartCoroutine(FadeCanvasGroup(textPanelCG, 1f, 0f, 0.1f, enableInteraction: false));
            isTextVisible = false;

            // Change tooltip string
            showTextTooltip.tooltipText = showTextString;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enableTextTooltip)
        {
            // Check for controller
            if (other.GetComponent<XRDirectInteractor>() != null)
            {
                isHovering = true;

                if (isDataSet)
                {
                    // Display Tooltip
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.ShowTooltip(showTextTooltip);

                    // Set Tooltip String
                    if (!isTextVisible)
                        showTextTooltip.tooltipText = showTextString;
                    else
                        showTextTooltip.tooltipText = hideTextString;

                    // Subscribe to Input Action
                    showTextInputAction.action.Enable();
                    showTextInputAction.action.performed -= ShowHideText;
                    showTextInputAction.action.performed += ShowHideText;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enableTextTooltip)
        {
            // Check for controller
            if (other.GetComponent<XRDirectInteractor>() != null)
            {
                isHovering = false;

                if (isDataSet)
                {
                    // Hide Tooltip
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.HideTooltip(showTextTooltip);

                    // Unsubscribe to Input Action
                    showTextInputAction.action.performed -= ShowHideText;
                    StartCoroutine(DisableInputActionAfterDelay(showTextInputAction));
                }
            }
        }
    }

    private IEnumerator DisableInputActionAfterDelay(InputActionReference inputAction)
    {
        yield return new WaitForSeconds(0.1f);

        inputAction.action.Disable();
    }


    private IEnumerator FadeCanvasGroup(CanvasGroup cG, float startAlpha, float endAlpha, float duration, float startDelay = 0f, bool enableInteraction = false)
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float t = 0f;
        while (t < duration)
        {
            cG.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);

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


    // Late join stuff

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetLateJoinInfo), newPlayer, exhibitNameString, exhibitInfoItemIndex, exhibitInfoContentType);
        }
    }

    [PunRPC]
    void SetLateJoinInfo(string exhibitName, int exhibitInfoIndex, int exhibitInfoContentType)
    {
        SetInfoFromExhibitInfoRPC(exhibitName, exhibitInfoIndex, exhibitInfoContentType);
    }
}
