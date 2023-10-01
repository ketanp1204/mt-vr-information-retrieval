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
    public string imagePrefabLoc = "UtilityPrefabs/ImagePrefab";


    // Private Variables //

    private TextMeshProUGUI detailInfoTextObject;
    private AudioSource detailInfoAudioSource;
    private Transform imageLocs;
    private Transform videoLocs;
    private Transform relatedItemLocs;
    private List<GameObject> detailViewSpawnedObjs = new List<GameObject>();



    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];
        gameObject.name = "DVA" + index.ToString();

        string itemName = (string)data[1];
        string dVName = "DV_" + itemName;

        // Get DV GameObject
        GameObject dVGO = transform.Find("DetailViews/" + dVName).gameObject;
        dVGO.SetActive(true);

        // Get info placement objects
        detailInfoTextObject = GetChildWithName(dVGO, "DetailInfoText").GetComponent<TextMeshProUGUI>();
        detailInfoAudioSource = GetChildWithName(dVGO, "DetailInfoAudioSource").GetComponent<AudioSource>();
        imageLocs = GetChildWithName(dVGO, "ImageLocs");
        videoLocs = GetChildWithName(dVGO, "VideoLocs");
        relatedItemLocs = GetChildWithName(dVGO, "RelatedItemLocs");

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

        // Set detail info audio 
        detailInfoAudioSource.clip = exhibitInfo.detailInfoAudio;

        // Spawn detail view images
        for (int i = 0; i < exhibitInfo.detailInfoImages.Length; i++)
        {
            GameObject image = PhotonNetwork.Instantiate(imagePrefabLoc, imageLocs.GetChild(i).transform.position, imageLocs.GetChild(i).transform.rotation);
            ImagePrefab iP = image.GetComponent<ImagePrefab>();
            iP.SetImage(exhibitInfo.detailInfoImages[i].image);
            iP.SetText(exhibitInfo.detailInfoImages[i].imageText.text);
            detailViewSpawnedObjs.Add(image);
        }

        // Spawn detail view videos
        for (int i = 0; i < exhibitInfo.detailInfoVideos.Length; i++)
        {
            
        }


        // Spawn detail view related items





        // Add focused objects after delay
        StartCoroutine(AddFocusedObjectsAfterDelay());
        


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

    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Vector3[] positions = new Vector3[syncObjects.Count];
        Quaternion[] rotations = new Quaternion[syncObjects.Count];
        Vector3[] scales = new Vector3[syncObjects.Count];

        for (int i = 0; i < syncObjects.Count; i++)
        {
            positions[i] = syncObjects[i].transform.position;
            rotations[i] = syncObjects[i].transform.rotation;
            scales[i] = syncObjects[i].transform.localScale;
        }

        // Write
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(positions);
            stream.SendNext(rotations);
            stream.SendNext(scales);
        }
        // Read
        else if (stream.IsReading)
        {
            positions = (Vector3[])stream.ReceiveNext();
            rotations = (Quaternion[])stream.ReceiveNext();
            scales = (Vector3[])stream.ReceiveNext();

            for (int i = 0; i < syncObjects.Count; i++)
            {
                syncObjects[i].transform.position = positions[i];
                syncObjects[i].transform.rotation = rotations[i];
                syncObjects[i].transform.localScale = scales[i];
            }
        }
    }
    */
}
