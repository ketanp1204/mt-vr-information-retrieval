using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class ImageInfo
{
    public Sprite image;
    public TextAsset imageText;
}

[System.Serializable]
public class VideoInfo
{
    public VideoClip videoClip;
    public TextAsset videoClipText;
    public Sprite videoClipThumbnail;
}

[System.Serializable]
public class RelatedItemInfo
{
    public GameObject model;
    public TextAsset modelText;
}

[CreateAssetMenu(menuName = "Exhibit Info")]
public class ExhibitInformation : ScriptableObject
{
    // Text 
    public TextAsset basicInfoText;
    public TextAsset detailInfoText;

    // Audio 
    public AudioClip basicInfoAudio;
    public AudioClip detailInfoAudio;

    // Images 
    public ImageInfo[] basicInfoImages;
    public ImageInfo[] detailInfoImages;

    // Videos
    public VideoInfo[] detailInfoVideos;

    // Related Items
    public RelatedItemInfo[] detailInfoRelatedItems;
}
