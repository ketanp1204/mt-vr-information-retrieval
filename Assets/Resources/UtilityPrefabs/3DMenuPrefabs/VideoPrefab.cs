using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

public class VideoPrefab : MonoBehaviourPunCallbacks
{

    // Public Variables //

    public Image imageComp;
    public CanvasGroup textPanelCG;
    public TextMeshProUGUI textField;
    public VideoPlayer videoPlayer;
    public GameObject videoPlayerQuad;
    public Image videoPlayerThumbnail;
    public Tooltip showTextTooltip;
    public Tooltip playPauseTooltip;
    public Tooltip stopTooltip;
    public InputActionReference showTextInputAction;
    public InputActionReference playPauseInputAction;
    public InputActionReference stopInputAction;


    // Private Variables

    private bool isHoveringLeft = false;
    private bool isHoveringRight = false;
    private bool enableTextTooltip = false;
    private bool enableMediaTooltips = false;
    private bool isTextVisible = false;
    private bool isDataSet = false;
    private TooltipHandler tooltipHandler;
    private string showTextString = "Show Info";
    private string hideTextString = "Hide Info";
    private string gOName = "DVVideos";


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
        photonView.RPC(nameof(SetInfoFromExhibitInfoRPC), RpcTarget.Others, exhibitName, index, contentType);
    }

    [PunRPC]
    void SetInfoFromExhibitInfoRPC(string exhibitName, int index, int contentType)
    {
        // Get exhibit information object
        ExhibitInformation exhibitInfo = null;
        ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
        for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
        {
            if (exhibitInfoRefs.exhibitInfos[i].exhibitName == exhibitName)
            {
                exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
            }
        }

        // Set values
        if (contentType == 1)
        {
            gameObject.name = "DVVideos" + index.ToString();
            SetThumbnail(exhibitInfo.detailInfoVideos[index].videoClipThumbnail);
            SetVideoClip(exhibitInfo.detailInfoVideos[index].videoClip);
            SetText(exhibitInfo.detailInfoVideos[index].videoClipText.text);
        }
        else if (contentType == 2)
        {
            gameObject.name = "DVRelatedItems" + index.ToString();
            SetThumbnail(exhibitInfo.detailInfoRelatedItems[index].videoInfo.videoClipThumbnail);
            SetVideoClip(exhibitInfo.detailInfoRelatedItems[index].videoInfo.videoClip);
            SetText(exhibitInfo.detailInfoRelatedItems[index].videoInfo.videoClipText.text);
        }

        // Update GameObject name
        gOName = gOName + index.ToString();
        gameObject.name = gOName;
    }

    public void ShowHideText(InputAction.CallbackContext obj)
    {
        if (isHoveringRight)
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
        }
    }

    public void PlayPauseVideo(InputAction.CallbackContext obj)
    {
        if (isHoveringLeft)
        {
            // Hide Video Thumbnail
            videoPlayerThumbnail.enabled = false;

            // Play/Pause Video
            videoPlayerQuad.SetActive(true);
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
        if (isHoveringLeft)
        {
            // Stop Playing
            videoPlayer.Stop();
            videoPlayerQuad.SetActive(false);

            // Show Video Thumbnail
            videoPlayerThumbnail.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Text tooltip
        if (enableTextTooltip)
        {
            // Check for right controller
            if (other.GetComponent<XRDirectInteractor>() != null && other.gameObject.name.Contains("Right"))
            {
                isHoveringRight = true;

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

        // Media tooltips
        if (enableMediaTooltips)
        {
            // Check for left controller
            if (other.GetComponent<XRDirectInteractor>() != null && other.gameObject.name.Contains("Left"))
            {
                isHoveringLeft = true;

                if (isDataSet)
                {
                    // Display Tooltips
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.ShowTooltip(playPauseTooltip);
                    tooltipHandler.ShowTooltip(stopTooltip);

                    // Subscribe to Input Actions
                    playPauseInputAction.action.Enable();
                    stopInputAction.action.Enable();
                    playPauseInputAction.action.performed -= PlayPauseVideo;
                    playPauseInputAction.action.performed += PlayPauseVideo;
                    stopInputAction.action.performed -= StopVideo;
                    stopInputAction.action.performed += StopVideo;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Text tooltip
        if (enableTextTooltip)
        {
            // Check for right controller
            if (other.GetComponent<XRDirectInteractor>() != null && other.gameObject.name.Contains("Right"))
            {
                isHoveringRight = false;
                
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

        // Media tooltips
        if (enableMediaTooltips)
        {
            // Check for left controller
            if (other.GetComponent<XRDirectInteractor>() != null && other.gameObject.name.Contains("Left"))
            {
                isHoveringLeft = false;

                if (isDataSet)
                {
                    // Hide Tooltips
                    tooltipHandler = other.transform.root.GetComponent<TooltipHandler>();
                    tooltipHandler.HideTooltip(playPauseTooltip);
                    tooltipHandler.HideTooltip(stopTooltip);

                    // Unsubscribe to Input Actions
                    playPauseInputAction.action.performed -= PlayPauseVideo;
                    stopInputAction.action.performed -= StopVideo;
                    StartCoroutine(DisableInputActionAfterDelay(playPauseInputAction));
                    StartCoroutine(DisableInputActionAfterDelay(stopInputAction));
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
}
