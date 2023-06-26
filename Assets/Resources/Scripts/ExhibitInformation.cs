using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Exhibit Information")]
public class ExhibitInformation : ScriptableObject
{
    public TextAsset description;
    public AudioClip audioGuide;
    public Sprite[] images;
    public VideoClip[] videos;
}
