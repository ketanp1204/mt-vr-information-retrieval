using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;

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
        interactable.colliders.Add(modelGO.GetComponent<BoxCollider>());
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
        SetExhibitInfo(exhibitName, index);

        photonView.RPC(nameof(SetInfoFromExhibitInfoRPC), RpcTarget.Others, index);
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
    void SetInfoFromExhibitInfoRPC(int index)
    {
        // Set exhibit info values
        gameObject.name = "DVRelatedItems" + index.ToString();
        SetModel(exhibitInfo.detailInfoRelatedItems[index].modelInfo.model);
        SetText(exhibitInfo.detailInfoRelatedItems[index].modelInfo.modelText.text);

        // Update GameObject name
        gOName = "DVRelatedItems" + index.ToString();
        gameObject.name = gOName;
    }

    public void ShowText(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            StartCoroutine(FadeCanvasGroup(textPanelCG, 0f, 1f, 0.1f, enableInteraction: true));
            isTextVisible = true;

            // Remove show text input action
            showTextInputAction.action.performed -= ShowText;

            // Change tooltip string
            showTextTooltip.tooltipText = hideTextString;

            // Add hide text input action
            showTextInputAction.action.performed += HideText;
        }
    }

    public void HideText(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            StartCoroutine(FadeCanvasGroup(textPanelCG, 1f, 0f, 0.1f, enableInteraction: false));
            isTextVisible = false;

            // Remove hide text input action
            showTextInputAction.action.performed -= HideText;

            // Change tooltip string
            showTextTooltip.tooltipText = showTextString;

            // Add show text input action
            showTextInputAction.action.performed += ShowText;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for controller
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            isHovering = true;

            if (enableTextTooltip)
            {
                tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                tooltipHandler.ShowTooltip(showTextTooltip);

                showTextInputAction.action.Enable();
                if (!isTextVisible)
                {
                    showTextTooltip.tooltipText = showTextString;
                    showTextInputAction.action.performed -= ShowText;
                    showTextInputAction.action.performed += ShowText;
                }
                else
                {
                    showTextTooltip.tooltipText = hideTextString;
                    showTextInputAction.action.performed -= HideText;
                    showTextInputAction.action.performed += HideText;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check for controller
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            isHovering = false;

            if (enableTextTooltip)
            {
                tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                tooltipHandler.HideTooltip(showTextTooltip);

                showTextInputAction.action.Disable();
                if (!isTextVisible)
                    showTextInputAction.action.performed -= ShowText;
                else
                    showTextInputAction.action.performed -= HideText;
            }
        }
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
        photonView.RPC(nameof(SetLateJoinInfo), newPlayer, exhibitNameString, exhibitInfoItemIndex);
    }

    [PunRPC]
    void SetLateJoinInfo(string exhibitName, int exhibitInfoIndex)
    {
        SetExhibitInfo(exhibitName, exhibitInfoIndex);

        SetInfoFromExhibitInfoRPC(exhibitInfoIndex);
    }
}
