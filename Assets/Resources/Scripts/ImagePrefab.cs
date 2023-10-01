using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ImagePrefab : MonoBehaviour
{

    // Public Variables //

    public TextMeshProUGUI textField;
    public Tooltip showTextTooltip;
    public InputActionReference primaryButton;


    // Private Variables

    private bool isHovering = false;
    private bool enableTextTooltip = false;
    private bool isTextVisible = false;
    private TooltipHandler tooltipHandler;


    
    public void SetImage(Sprite image)
    {
        // Set and resize image on the child
        Transform child = transform.GetChild(0);
        Image imageComp = child.GetComponent<Image>();
        imageComp.sprite = image;
        float aspectRatio = image.rect.width / image.rect.height;
        var fitter = child.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = aspectRatio;

        // Resize box collider
        BoxCollider c = child.GetComponent<BoxCollider>();
        RectTransform rt = child.GetComponent<RectTransform>();
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
            Transform child = transform.GetChild(1);
            StartCoroutine(FadeCanvasGroup(child.GetComponent<CanvasGroup>(), 0f, 1f, 0.1f, enableInteraction: true));
            isTextVisible = true;
        }
    }

    public void HideText(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            Transform child = transform.GetChild(1);
            StartCoroutine(FadeCanvasGroup(child.GetComponent<CanvasGroup>(), 1f, 0f, 0.1f, enableInteraction: false));
            isTextVisible = false;
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

                primaryButton.action.Enable();
                if (!isTextVisible)
                    primaryButton.action.performed += ShowText;
                else
                    primaryButton.action.performed += HideText;
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

                primaryButton.action.Disable();
                if (!isTextVisible)
                    primaryButton.action.performed -= ShowText;
                else
                    primaryButton.action.performed -= HideText;
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
