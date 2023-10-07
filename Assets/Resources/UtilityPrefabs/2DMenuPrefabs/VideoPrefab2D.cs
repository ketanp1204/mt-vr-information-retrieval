using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPrefab2D : MonoBehaviour
{
    // Public Variables //

    public Image imageComp;
    public Button imageButton;


    // Private Variables //

    private float imageHeight = 22f;


    public void SetData(VideoClip videoClip, Sprite videoThumbnail, string videoText, TextBox textBox, GameObject videoPlayerBox, VideoPlayer videoPlayer)
    {
        // Set and resize image on the child
        imageComp.sprite = videoThumbnail;
        imageComp.SetNativeSize();
        float aspectRatio = imageComp.rectTransform.rect.width / imageComp.rectTransform.rect.height;

        // Resize parent rect
        float rectWidth = imageHeight * aspectRatio;
        RectTransform rT = (RectTransform)transform;
        rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectWidth);
        rT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight);

        // Rescale image component gameObject rect
        imageComp.GetComponent<RectTransform>().localScale = Vector3.one * (imageHeight / imageComp.rectTransform.rect.height);

        // Set display text on button click event
        imageButton.onClick.AddListener(() => { textBox.DisplayText(videoText); });

        // Set video clip on player on button click event
        imageButton.onClick.AddListener(() => { videoPlayer.clip = videoClip; });
        imageButton.onClick.AddListener(() => { videoPlayerBox.SetActive(true); });
        imageButton.onClick.AddListener(() => { videoPlayer.Play(); });
    }
}
