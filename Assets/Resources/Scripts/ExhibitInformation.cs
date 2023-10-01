using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public struct ImageInfo
{
    public Sprite image;
    public TextAsset imageText;
}

[System.Serializable]
public struct VideoInfo
{
    public VideoClip videoClip;
    public TextAsset videoClipText;
    public Sprite videoClipThumbnail;
}

[System.Serializable]
public struct ModelInfo
{
    public GameObject model;
    public TextAsset modelText;
}

[System.Serializable]
public struct RelatedItemInfo
{
    public ModelInfo modelInfo;
    public ImageInfo imageInfo;
    public VideoInfo videoInfo;
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
