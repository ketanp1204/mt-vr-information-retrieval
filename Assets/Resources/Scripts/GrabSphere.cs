using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using Photon.Pun;

public class GrabSphere : MonoBehaviourPunCallbacks
{
    // Public Variables
    public MeshRenderer rend;

    public void SetVisibility(bool visible)
    {
        photonView.RPC(nameof(SetVisibilityRPC), RpcTarget.All, visible);
    }

    [PunRPC]
    void SetVisibilityRPC(bool visible)
    {
        if (!photonView.IsMine)
        {
            rend.enabled = visible;
        }
    }
}
