using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class UserDisplaySync : MonoBehaviourPunCallbacks, IPunObservable
{
    Vector3 receivedPosition;
    Vector3 receivedRotation;

    public void SyncUserDisplay(bool activeState)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SyncActiveState", RpcTarget.AllBuffered, activeState);
        }
    }

    [PunRPC]
    void SyncActiveState(bool activeState)
    {
        gameObject.SetActive(activeState);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(gameObject.transform.position);
            stream.SendNext(gameObject.transform.rotation);
        }
        else if (stream.IsReading)
        {
            receivedPosition = (Vector3)stream.ReceiveNext();
            receivedRotation = (Vector3)stream.ReceiveNext();
        }
    }
}
