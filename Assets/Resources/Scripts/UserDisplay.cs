using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class UserDisplay : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    /* Public Variables */
    public Vector3 detailViewingAreaPosition;
    public int interestGroup;
    public int dVAIndex;
    public DetailViewManager dVManager;

    /* Private Variables */
    private TMP_Text nameTag;
    private int viewID;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        photonView.Owner.TagObject = info.Sender.TagObject;

        nameTag = GetComponentInChildren<TMP_Text>();
    }

    public void SetUsernameWrapper(string name)
    {
        photonView.RPC(nameof(SetUsername), RpcTarget.AllBuffered, name);
    }

    public void SetDVAPositionWrapper(Vector3 t)
    {
        photonView.RPC(nameof(SetDVAPosition), RpcTarget.AllBuffered, t);
    }

    public void SetInterestGroup(int iG)
    {
        photonView.RPC(nameof(UpdateInterestGroup), RpcTarget.AllBuffered, iG);
    }

    public void SetDVAIndexWrapper(int index)
    {
        photonView.RPC(nameof(SetDVAIndex), RpcTarget.AllBuffered, index);
    }

    public void SetDVManagerWrapper(int id)
    {
        photonView.RPC(nameof(SetDVManager), RpcTarget.AllBuffered, id);
    }

    [PunRPC]
    void SetUsername(string name)
    {
        nameTag.text = name;
    }

    [PunRPC]
    void SetDVAPosition(Vector3 t)
    {
        detailViewingAreaPosition = t;
    }

    [PunRPC]
    void UpdateInterestGroup(int iG)
    {
        interestGroup = iG;
    }

    [PunRPC]
    void SetDVAIndex(int index)
    {
        dVAIndex = index;
    }

    [PunRPC]
    void SetDVManager(int id)
    {
        viewID = id;
        dVManager = PhotonView.Find(viewID).GetComponent<DetailViewManager>();
    }
}
