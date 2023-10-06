using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

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
    private TooltipHandler tooltipHandler;
    private string showTextString = "Show Info";
    private string hideTextString = "Hide Info";


    
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
    }

    public void SetText(string text)
    {
        textField.text = text;
        enableTextTooltip = true;
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
}
