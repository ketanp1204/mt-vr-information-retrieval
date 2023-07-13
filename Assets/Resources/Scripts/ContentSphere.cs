using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using Photon.Pun;

public class ContentSphere : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Public Variables
    public MeshRenderer rend;

    public void SetVisibility(bool visible)
    {
        photonView.RPC(nameof(SetVisibilityRPC), RpcTarget.All, visible);
    }

    public void DestroySphere()
    {
        photonView.RPC(nameof(DestroySphereRPC), RpcTarget.All);
    }

    [PunRPC]
    void DestroySphereRPC()
    {
        Destroy(gameObject);
    }

    [PunRPC]
    void SetVisibilityRPC(bool visible)
    {
        if (!photonView.IsMine)
        {
            rend.enabled = visible;
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // Hide the sphere for others
        if (!photonView.IsMine)
        {
            rend.enabled = false;
        }
    }
}
