using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DetailViewingArea : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];

        gameObject.name = "DVA" + index.ToString();
    }
}
