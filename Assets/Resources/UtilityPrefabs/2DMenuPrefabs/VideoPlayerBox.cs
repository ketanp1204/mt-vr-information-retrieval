using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerBox : MonoBehaviour
{
    // Public Variables //

    public PhotonView photonView;
    public GameObject videoPlayerQuad;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    // Private Variables //

    private CanvasGroup cG;



    // Start is called before the first frame update
    void Start()
    {
        cG = GetComponent<CanvasGroup>();
        cG.alpha = 0f;
        cG.interactable = false;
        cG.blocksRaycasts = false;

        videoPlayerQuad.SetActive(false);
    }

    public void PlayVideo(VideoClip clip, bool syncOverNetwork, int exhibitInfoVideoIndex)
    {
        if (cG.alpha == 0f)
        {
            // Show video player and buttons
            cG.alpha = 1f;
            cG.interactable = true;
            cG.blocksRaycasts = true;
            videoPlayerQuad.SetActive(true);

            // Set and Play Video
            videoPlayer.clip = clip;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            videoPlayer.Play();

            // Sync over network
            if(syncOverNetwork) 
                photonView.RPC("UpdateVideoPlayer", RpcTarget.Others, true, exhibitInfoVideoIndex);
        }
    }

    public void HideVideo(bool syncOverNetwork)
    {
        if (cG.alpha == 1f)
        {
            // Stop Video
            videoPlayer.Stop();

            // Hide video player and buttons
            cG.alpha = 0f;
            cG.interactable = false;
            cG.blocksRaycasts = false;
            videoPlayerQuad.SetActive(false);

            // Sync over network
            if (syncOverNetwork)
                photonView.RPC("UpdateVideoPlayer", RpcTarget.Others, false, 0);
        }
    }

    public void SetAndPlayVideo(VideoClip clip)
    {
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }

    public void PlayButton(bool syncOverNetwork)
    {
        videoPlayer.Play();
        if (syncOverNetwork)
        {
            photonView.RPC("VideoMediaControlsUpdate", RpcTarget.Others, true, false, false);
        }

    }

    public void PauseButton(bool syncOverNetwork)
    {
        videoPlayer.Pause();
        if (syncOverNetwork)
        {
            photonView.RPC("VideoMediaControlsUpdate", RpcTarget.Others, false, true, false);
        }
    }

    public void StopButton(bool syncOverNetwork)
    {
        videoPlayer.Stop();
        if (syncOverNetwork)
        {
            photonView.RPC("VideoMediaControlsUpdate", RpcTarget.Others, false, false, true);
        }
    }

    
}
