using Photon.Pun;
using Photon.Realtime;
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
    public List<GameObject> detailViewSpawnedObjs = new List<GameObject>();


    // Private Variables //

    private string imagePrefabLoc = "UtilityPrefabs/3DMenuPrefabs/ImagePrefab3D";
    private string videoPrefabLoc = "UtilityPrefabs/3DMenuPrefabs/VideoPrefab3D";
    private string modelPrefabLoc = "UtilityPrefabs/3DMenuPrefabs/ModelPrefab3D";
    private TextMeshProUGUI detailInfoTextObject;
    private AudioSource detailInfoAudioSource;
    private Transform imageLocs;
    private Transform videoLocs;
    private Transform relatedItemLocs;
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

        if (Vrsys.NetworkUser.localNetworkUser != null)
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
                photonView.RPC(nameof(UpdateDetailInfoTextRPC), RpcTarget.Others, itemName);

                // Set detail info audio 
                detailInfoAudioSource.clip = exhibitInfo.detailInfoAudio;
                photonView.RPC(nameof(UpdateDetailInfoAudioRPC), RpcTarget.Others, itemName);

                // Spawn detail view images
                for (int i = 0; i < exhibitInfo.detailInfoImages.Length; i++)
                {
                    // Instantiate Image Prefab
                    GameObject image = PhotonNetwork.Instantiate(imagePrefabLoc, imageLocs.GetChild(i).transform.position, imageLocs.GetChild(i).transform.rotation);
                    image.name = "DVImages" + i.ToString();

                    // Set Exhibit Info Data
                    ImagePrefab iP = image.GetComponent<ImagePrefab>();
                    iP.SetImage(exhibitInfo.detailInfoImages[i].image);
                    iP.SetText(exhibitInfo.detailInfoImages[i].imageText.text);                
                    iP.SetInfoFromExhibitInfo(itemName, i, 1);

                    // Add to detail view spawned objects list
                    photonView.RPC(nameof(AddToDVSpawnedObjectsList), RpcTarget.All, image.name);
                }

                // Spawn detail view videos
                for (int i = 0; i < exhibitInfo.detailInfoVideos.Length; i++)
                {
                    // Instantiate Video Prefab
                    GameObject video = PhotonNetwork.Instantiate(videoPrefabLoc, videoLocs.GetChild(i).transform.position, videoLocs.GetChild(i).transform.rotation);
                    video.name = "DVVideos" + i.ToString();

                    // Set Exhibit Info Data
                    VideoPrefab vP = video.GetComponent<VideoPrefab>();
                    vP.SetThumbnail(exhibitInfo.detailInfoVideos[i].videoClipThumbnail);
                    vP.SetText(exhibitInfo.detailInfoVideos[i].videoClipText.text);
                    vP.SetVideoClip(exhibitInfo.detailInfoVideos[i].videoClip);
                    vP.SetInfoFromExhibitInfo(itemName, i, 1);

                    // Add to detail view spawned objects list
                    photonView.RPC(nameof(AddToDVSpawnedObjectsList), RpcTarget.All, video.name);
                }


                // Spawn detail view related items
                for (int i = 0; i < exhibitInfo.detailInfoRelatedItems.Length; i++)
                {
                    // Item of type image
                    if (exhibitInfo.detailInfoRelatedItems[i].imageInfo.image != null)
                    {
                        // Instantiate Image Prefab
                        GameObject image = PhotonNetwork.Instantiate(imagePrefabLoc, relatedItemLocs.GetChild(i).transform.position, relatedItemLocs.GetChild(i).transform.rotation);
                        image.name = "DVRelatedItems" + i.ToString();

                        // Set Exhibit Info Data
                        ImagePrefab iP = image.GetComponent<ImagePrefab>();
                        iP.SetImage(exhibitInfo.detailInfoRelatedItems[i].imageInfo.image);
                        iP.SetText(exhibitInfo.detailInfoRelatedItems[i].imageInfo.imageText.text);
                        iP.SetInfoFromExhibitInfo(itemName, i, 2);

                        // Add to detail view spawned objects list
                        photonView.RPC(nameof(AddToDVSpawnedObjectsList), RpcTarget.All, image.name);
                    }

                    // Item of type video
                    if (exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClip != null)
                    {
                        // Instantiate Video Prefab
                        GameObject video = PhotonNetwork.Instantiate(videoPrefabLoc, relatedItemLocs.GetChild(i).transform.position, relatedItemLocs.GetChild(i).transform.rotation);
                        video.name = "DVRelatedItems" + i.ToString();

                        // Set Exhibit Info Data
                        VideoPrefab vP = video.GetComponent<VideoPrefab>();
                        vP.SetThumbnail(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipThumbnail);
                        vP.SetText(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClipText.text);
                        vP.SetVideoClip(exhibitInfo.detailInfoRelatedItems[i].videoInfo.videoClip);
                        vP.SetInfoFromExhibitInfo(itemName, i, 2);

                        // Add to detail view spawned objects list
                        photonView.RPC(nameof(AddToDVSpawnedObjectsList), RpcTarget.All, video.name);
                    }

                    // Item of type model
                    if (exhibitInfo.detailInfoRelatedItems[i].modelInfo.model != null)
                    {
                        // Instantiate Model Prefab
                        GameObject model = PhotonNetwork.Instantiate(modelPrefabLoc, relatedItemLocs.GetChild(i).transform.position, relatedItemLocs.GetChild(i).transform.rotation);
                        model.name = "DVRelatedItems" + i.ToString();

                        // Set Exhibit Info Data
                        ModelPrefab mP = model.GetComponent<ModelPrefab>();
                        mP.SetModel(exhibitInfo.detailInfoRelatedItems[i].modelInfo.model);
                        mP.SetText(exhibitInfo.detailInfoRelatedItems[i].modelInfo.modelText.text);
                        mP.SetInfoFromExhibitInfo(itemName, i);

                        // Add to detail view spawned objects list
                        photonView.RPC(nameof(AddToDVSpawnedObjectsList), RpcTarget.All, model.name);
                    }
                }

            // Add focused objects after delay
            StartCoroutine(AddFocusedObjectsAfterDelay());
        }
    }

    //void 

    [PunRPC]
    void UpdateDetailInfoTextRPC(string itemName)
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
    void UpdateDetailInfoAudioRPC(string itemName)
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

    [PunRPC]
    void AddToDVSpawnedObjectsList(string gOName)
    {
        detailViewSpawnedObjs.Add(GameObject.Find(gOName));
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
