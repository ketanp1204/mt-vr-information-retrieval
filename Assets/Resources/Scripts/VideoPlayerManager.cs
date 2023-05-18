using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    /* Public Variables */
    public VideoPlayer videoPlayer;
    public VideoProgressBar videoProgressBar;

    /* Private Variables */

    // Start is called before the first frame update
    void Start()
    {
        videoProgressBar.videoPlayer = videoPlayer;
    }

    public void ToggleVideoPlayer(bool toggle)
    {
        videoPlayer.gameObject.SetActive(toggle);
    }

    public void SetVideoClip(VideoClip clip)
    {
        if (videoPlayer != null)
        {
            videoPlayer.clip = clip;
        }
    }

    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
}
