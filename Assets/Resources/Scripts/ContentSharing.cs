using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ContentSharing : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public GameObject visibilityObject;

    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // Hide the content for others
        if (!photonView.IsMine)
        {
            visibilityObject.SetActive(false);
        }
    }
}
