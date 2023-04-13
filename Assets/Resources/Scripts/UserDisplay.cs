using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using JetBrains.Annotations;

public class UserDisplay : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    /* Public Variables */
    public Vector3 detailViewingAreaPosition;
    public int interestGroup;

    /* Private Variables */
    private TMP_Text nameTag;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        photonView.Owner.TagObject = info.Sender.TagObject;

        nameTag = GetComponentInChildren<TMP_Text>();
    }

    public void SetUsername(string name)
    {
        photonView.RPC(nameof(UpdateUsername), RpcTarget.AllBuffered, name);
    }

    public void SetDVAPosition(Vector3 t)
    {
        photonView.RPC(nameof(UpdateDVAPosition), RpcTarget.AllBuffered, t);
    }

    public void SetInterestGroup(int iG)
    {
        photonView.RPC(nameof(UpdateInterestGroup), RpcTarget.AllBuffered, iG);
    }

    [PunRPC]
    void UpdateUsername(string name)
    {
        nameTag.text = name;
    }

    [PunRPC]
    void UpdateDVAPosition(Vector3 t)
    {
        detailViewingAreaPosition = t;
    }

    [PunRPC]
    void UpdateInterestGroup(int iG)
    {
        interestGroup = iG;
    }
}
