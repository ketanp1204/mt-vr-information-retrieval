using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class UserDisplay : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{

    /* Private Variables */
    private TMP_Text nameTag;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        photonView.Owner.TagObject = info.Sender.TagObject;

        nameTag = GetComponentInChildren<TMP_Text>();
        Debug.Log(nameTag);
    }

    public void SetUsername(string name)
    {
        photonView.RPC(nameof(UpdateUsername), RpcTarget.AllBuffered, name);
    }

    [PunRPC]
    void UpdateUsername(string name)
    {
        nameTag.text = name;
    }
    
}
