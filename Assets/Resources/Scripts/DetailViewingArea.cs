using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class DetailViewingArea : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];
        gameObject.name = "DVA" + index.ToString();

        string objName = (string)data[1];
        int viewID = (int)data[2];

        objName = "DV_" + objName;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.name == objName)
            {
                child.transform.Find("ExitSphere").GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                    (SelectEnterEventArgs args) => { PhotonView.Find(viewID).GetComponent<DetailViewManager>().HandleSelect(); });
            }
        }
    }
}
