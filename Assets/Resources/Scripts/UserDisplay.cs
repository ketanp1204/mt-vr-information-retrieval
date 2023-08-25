using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Photon.Realtime;

public class UserDisplay : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    /* Public Variables */
    public int dVAIndex;
    public string dVAObject;
    public string itemName;

    /* Private Variables */
    private TMP_Text nameTag;
    private int viewID;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        photonView.Owner.TagObject = info.Sender.TagObject;

        nameTag = GetComponentInChildren<TMP_Text>();
    }

    public void SetUsername(string name)
    {
        photonView.RPC(nameof(SetUsernameRPC), RpcTarget.All, name);
    }

    public void SetDVAIndex(int index)
    {
        photonView.RPC(nameof(SetDVAIndexRPC), RpcTarget.All, index);
    }

    public void SetDVAObject(string name)
    {
        photonView.RPC(nameof(SetDVAObjectRPC), RpcTarget.All, name);
    }

    public void SetItemName(string name)
    {
        photonView.RPC(nameof(SetItemNameRPC), RpcTarget.All, name);
    }

    [PunRPC]
    void SetUsernameRPC(string name)
    {
        nameTag.text = name;
    }

    [PunRPC]
    void SetDVAIndexRPC(int index)
    {
        dVAIndex = index;
    }

    [PunRPC]
    void SetDVAObjectRPC(string name)
    {
        dVAObject = name;
    }

    [PunRPC]
    void SetItemNameRPC(string name)
    {
        itemName = name;
    }

    // Late join stuff

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetUsernameRPC), newPlayer, nameTag.text);

            photonView.RPC(nameof(SetDVAIndexRPC), newPlayer, dVAIndex);

            photonView.RPC(nameof(SetDVAObjectRPC), newPlayer, dVAObject);

            photonView.RPC(nameof(SetItemNameRPC), newPlayer, itemName);
        }
    }
}
