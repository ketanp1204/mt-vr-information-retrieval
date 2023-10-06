using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DVAObject : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Public Variables //

    public GameObject exitSpherePrefab;
    public GameObject dVContainer;
    public List<GameObject> dVObjectPrefabs;
    public List<Transform> syncObjects;



    // Private Variables //

    private string imagePrefabLoc = "UtilityPrefabs/3DMenuPrefabs/ImagePrefab3D";
    private string videoPrefabLoc = "UtilityPrefabs/3DMenuPrefabs/VideoPrefab3D";
    private TextMeshProUGUI detailInfoTextObject;
    private AudioSource detailInfoAudioSource;
    private Transform imageLocs;
    private Transform videoLocs;
    private Transform relatedItemLocs;
    public List<GameObject> detailViewSpawnedObjs = new List<GameObject>();
    private string itemName;
    private string dVName;
    private GameObject dVGO = null;



    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];
        gameObject.name = "DVA" + index.ToString();

        itemName = (string)data[1];
        dVName = "DV_" + itemName;

        int viewID = (int)data[2];

        // Get DV GameObject
        dVGO = transform.Find("DetailViews/" + dVName).gameObject;
        dVGO.SetActive(true);

        // Get info placement objects
        detailInfoTextObject = GetChildWithName(dVGO, "DetailInfoText").GetComponent<TextMeshProUGUI>();
        detailInfoAudioSource = GetChildWithName(dVGO, "DetailInfoAudioSource").GetComponent<AudioSource>();
        imageLocs = GetChildWithName(dVGO, "ImageLocs");
        videoLocs = GetChildWithName(dVGO, "VideoLocs");
        relatedItemLocs = GetChildWithName(dVGO, "RelatedItemLocs");

        if (Vrsys.NetworkUser.localNetworkUser.photonView.ViewID == viewID)
        {
            

            // Get exhibit information object
            ExhibitInformation exhibitInfo = null;
            ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
            for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
            {
                if (exhibitInfoRefs.exhibitInfos[i].exhibitName == itemName)
                {
                    exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
                }
            }

            // Set detail view text
            detailInfoTextObject.text = exhibitInfo.detailInfoText.text;
            photonView.RPC(nameof(UpdateDetailInfoText), RpcTarget.Others, itemName);

            // Set detail info audio 
            detailInfoAudioSource.clip = exhibitInfo.detailInfoAudio;
            photonView.RPC(nameof(UpdateDetailInfoAudio), RpcTarget.Others, itemName);

            // Spawn detail view images
            for (int i = 0; i < exhibitInfo.detailInfoImages.Length; i++)
            {
                GameObject image = PhotonNetwork.Instantiate(imagePrefabLoc, imageLocs.GetChild(i).transform.position, imageLocs.GetChild(i).transform.rotation);
                ImagePrefab iP = image.GetComponent<ImagePrefab>();
                iP.SetImage(exhibitInfo.detailInfoImages[i].image);
                iP.SetText(exhibitInfo.detailInfoImages[i].imageText.text);
                detailViewSpawnedObjs.Add(image);
                iP.SetInfoFromExhibitInfo(itemName, i, 0);
            }

            // Spawn detail view videos
            for (int i = 0; i < exhibitInfo.detailInfoVideos.Length; i++)
            {
                GameObject video = PhotonNetwork.Instantiate(videoPrefabLoc, videoLocs.GetChild(i).transform.position, videoLocs.GetChild(i).transform.rotation);
                VideoPrefab vP = video.GetComponent<VideoPrefab>();
                vP.SetThumbnail(exhibitInfo.detailInfoVideos[i].videoClipThumbnail);
                vP.SetText(exhibitInfo.detailInfoVideos[i].videoClipText.text);
                vP.SetVideoClip(exhibitInfo.detailInfoVideos[i].videoClip);
                detailViewSpawnedObjs.Add(video);
                vP.SetInfoFromExhibitInfo(itemName, i, 0);
            }


            // Spawn detail view related items
            for (int i = 0; i < exhibitInfo.detailInfoRelatedItems.Length; i++)
            {
                // Item of type image
                if (exhibitInfo.detailInfoRelatedItems[i].imageInfo.image != null)
                {
                    GameObject image = PhotonNetwork.Instantiate(imagePrefabLoc, relatedItemLocs.GetChild(i).transform.position, relatedItemLocs.GetChild(i).transform.rotation);
                    ImagePrefab iP = image.GetComponent<ImagePrefab>();
                    iP.SetImage(exhibitInfo.detailInfoRelatedItems[i].imageInfo.image);
                    iP.SetText(exhibitInfo.detailInfoRelatedItems[i].imageInfo.imageText.text);
                    detailViewSpawnedObjs.Add(image);
                    iP.SetInfoFromExhibitInfo(itemName, i, 1);
                }

                // Item of type video
                if (exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClip != null)
                {
                    GameObject video = PhotonNetwork.Instantiate(videoPrefabLoc, relatedItemLocs.GetChild(i).transform.position, relatedItemLocs.GetChild(i).transform.rotation);
                    VideoPrefab vP = video.GetComponent<VideoPrefab>();
                    vP.SetThumbnail(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipThumbnail);
                    vP.SetText(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipText.text);
                    vP.SetVideoClip(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClip);
                    detailViewSpawnedObjs.Add(video);
                    vP.SetInfoFromExhibitInfo(itemName, i, 1);
                }

                // Item of type model
                if (exhibitInfo.detailInfoRelatedItems[i].modelInfo.model != null)
                {

                }
            }

            // Add focused objects after delay
            StartCoroutine(AddFocusedObjectsAfterDelay());
        }
        /*
        // Spawn detail view items
        foreach (GameObject gO in dVObjectPrefabs)
        {
            if (gO.name == dVName)
            {
                StartCoroutine(InstantiateDVAfterDelay(gO));
            }
        }
        */
    }

    [PunRPC]
    void UpdateDetailInfoText(string itemName)
    {
        // Get exhibit information object
        ExhibitInformation exhibitInfo = null;
        ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
        for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
        {
            if (exhibitInfoRefs.exhibitInfos[i].exhibitName == itemName)
            {
                exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
            }
        }

        detailInfoTextObject.text = exhibitInfo.detailInfoText.text;
    }

    [PunRPC]
    void UpdateDetailInfoAudio(string itemName)
    {
        // Get exhibit information object
        ExhibitInformation exhibitInfo = null;
        ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
        for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
        {
            if (exhibitInfoRefs.exhibitInfos[i].exhibitName == itemName)
            {
                exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
            }
        }

        detailInfoAudioSource.clip = exhibitInfo.detailInfoAudio;
    }

    public void RemoveSpawnedObjects()
    {
        photonView.RPC(nameof(RemoveSpawnedObjectsRPC), RpcTarget.All);
    }

    [PunRPC]
    void RemoveSpawnedObjectsRPC()
    {
        foreach (GameObject gO in detailViewSpawnedObjs)
            Destroy(gO);
    }

    private IEnumerator InstantiateDVAfterDelay(GameObject gO)
    {
        yield return new WaitForSeconds(0.1f);

        // GameObject dV = PhotonNetwork.Instantiate("UtilityPrefabs/DetailViewObjects/" + gO.name, transform.position, gO.transform.rotation);
        GameObject dV = GameObject.Instantiate(gO, dVContainer.transform);

        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> objs = new List<GameObject>();
        objs.Add(dV);
        focus.AddFocused(objs);
    }

    private IEnumerator AddFocusedObjectsAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        // Add focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.AddFocused(detailViewSpawnedObjs);
    }

    private Transform GetChildWithName(GameObject gO, string childName)
    {
        Transform child = null;
        foreach (Transform t in gO.GetComponentsInChildren<Transform>())
        {
            if (t.name == childName)
            {
                child = t;
                break;
            }
        }
        return child;
    }
}
