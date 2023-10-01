using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

public class VideoPrefab : MonoBehaviour
{

    // Public Variables //

    public Image imageComp;
    public CanvasGroup textPanelCG;
    public TextMeshProUGUI textField;
    public VideoPlayer videoPlayer;
    public Tooltip showTextTooltip;
    public Tooltip playPauseTooltip;
    public Tooltip stopTooltip;
    public InputActionReference showTextInputAction;
    public InputActionReference playPauseInputAction;
    public InputActionReference stopInputAction;


    // Private Variables

    private bool isHovering = false;
    private bool enableTextTooltip = false;
    private bool enableMediaTooltips = false;
    private bool isTextVisible = false;
    private bool isVideoVisible = false;
    private TooltipHandler tooltipHandler;
    private string showTextString = "Show Info";
    private string hideTextString = "Hide Info";


    public void SetThumbnail(Sprite thumbnail)
    {
        // Set and resize image on the child
        imageComp.sprite = thumbnail;
        float aspectRatio = thumbnail.rect.width / thumbnail.rect.height;
        var fitter = imageComp.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = aspectRatio;

        // Resize box collider
        BoxCollider c = imageComp.GetComponent<BoxCollider>();
        RectTransform rt = imageComp.GetComponent<RectTransform>();
        c.size = new Vector3(rt.rect.width, rt.rect.height, c.size.z);
    }

    public void SetVideoClip(VideoClip clip)
    {
        videoPlayer.clip = clip;
        enableMediaTooltips = true;
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
            playPauseInputAction.action.performed -= ShowText;

            // Change tooltip string
            showTextTooltip.tooltipText = hideTextString;

            // Add hide text input action
            playPauseInputAction.action.performed += HideText;
        }
    }

    public void HideText(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            StartCoroutine(FadeCanvasGroup(textPanelCG, 1f, 0f, 0.1f, enableInteraction: false));
            isTextVisible = false;

            // Remove hide text input action
            playPauseInputAction.action.performed -= HideText;

            // Change tooltip string
            showTextTooltip.tooltipText = showTextString;

            // Add show text input action
            playPauseInputAction.action.performed += ShowText;
        }
    }

    public void PlayPauseVideo(InputAction.CallbackContext obj)
    {
        if (isHovering)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
    }

    public void StopVideo(InputAction.CallbackContext obj)
    {
        if (!isHovering)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for controller
        if (other.GetComponent<XRDirectInteractor>() != null)
        {
            isHovering = true;

            if (other.name.Contains("Right"))
            {
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
            else
            {
                if (enableMediaTooltips)
                {
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.ShowTooltip(playPauseTooltip);
                    tooltipHandler.ShowTooltip(stopTooltip);

                    playPauseInputAction.action.Enable();
                    stopInputAction.action.Enable();
                    if (isVideoVisible)
                    {
                        playPauseInputAction.action.performed += PlayPauseVideo;
                        stopInputAction.action.performed += StopVideo;
                    }
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

            if (other.name.Contains("Right"))
            {
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
            else
            {
                if (enableMediaTooltips)
                {
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.HideTooltip(playPauseTooltip);
                    tooltipHandler.HideTooltip(stopTooltip);

                    playPauseInputAction.action.Disable();
                    stopInputAction.action.Disable();
                    if (isVideoVisible)
                    {
                        playPauseInputAction.action.performed -= PlayPauseVideo;
                        stopInputAction.action.performed -= StopVideo;
                    }
                }
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
