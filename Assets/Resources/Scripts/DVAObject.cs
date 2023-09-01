using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DVAObject : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Public Variables //

    public GameObject exitSpherePrefab;
    public GameObject dVContainer;
    public List<GameObject> dVObjectPrefabs;
    public List<Transform> syncObjects;




    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];
        gameObject.name = "DVA" + index.ToString();

        string itemName = (string)data[1];
        string dVName = "DV_" + itemName;

        // Spawn detail view items
        foreach (GameObject gO in dVObjectPrefabs)
        {
            if (gO.name == dVName)
            {
                StartCoroutine(InstantiateDVAfterDelay(gO));
            }
        }
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
