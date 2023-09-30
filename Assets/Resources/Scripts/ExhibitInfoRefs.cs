using UnityEngine;

[System.Serializable]
public class ExhibitInfoRef
{
    public string exhibitName;
    public ExhibitInformation exhibitInfo;
}

[CreateAssetMenu(menuName = "Exhibit Info References")]
public class ExhibitInfoRefs : ScriptableObject
{
    public ExhibitInfoRef[] exhibitInfos;
}