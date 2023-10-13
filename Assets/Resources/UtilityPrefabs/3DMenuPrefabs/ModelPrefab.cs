using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using System.Data;

public class ModelPrefab : MonoBehaviourPunCallbacks
{
    // Public Variables //

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
    private string gOName = "DVVideos";
    private ExhibitInformation exhibitInfo = null;
    private string exhibitNameString = "";
    private int exhibitInfoItemIndex = 0;


    public void SetModel(GameObject model)
    {
        GameObject modelGO = Instantiate(model, transform.Find("Mesh"));

        // Add collider to XR Grab Interactable
        var interactable = GetComponent<XRGrabInteractable>();

        BoxCollider boxCollider = null;
        foreach (Transform t in modelGO.GetComponentsInChildren<Transform>())
        {
            if (t.GetComponent<BoxCollider>() != null)
            {
                boxCollider = t.GetComponent<BoxCollider>();
                break;
            }
        }
        
        interactable.colliders.Add(boxCollider);
        interactable.interactionManager.UnregisterInteractable(interactable.GetComponent<IXRInteractable>());
        interactable.interactionManager.RegisterInteractable(interactable.GetComponent<IXRInteractable>());
    }

    public void SetText(string text)
    {
        textField.text = text;
        enableTextTooltip = true;
    }

    public void SetInfoFromExhibitInfo(string exhibitName, int index)
    {
        photonView.RPC(nameof(SetInfoFromExhibitInfoRPC), RpcTarget.All, exhibitName, index);
    }

    private void SetExhibitInfo(string exhibitName, int index)
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
    }

    [PunRPC]
    void SetInfoFromExhibitInfoRPC(string exhibitName, int index)
    {
        SetExhibitInfo(exhibitName, index);

        // Set exhibit info values
        gameObject.name = "DVRelatedItems" + index.ToString();
        SetModel(exhibitInfo.detailInfoRelatedItems[index].modelInfo.model);
        SetText(exhibitInfo.detailInfoRelatedItems[index].modelInfo.modelText.text);

        // Update GameObject name
        gOName = "DVRelatedItems" + index.ToString();
        gameObject.name = gOName;
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
            photonView.RPC(nameof(SetLateJoinInfo), newPlayer, exhibitNameString, exhibitInfoItemIndex);
        }
    }

    [PunRPC]
    void SetLateJoinInfo(string exhibitName, int exhibitInfoIndex)
    {
        SetInfoFromExhibitInfoRPC(exhibitName, exhibitInfoIndex);
    }
}
