
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using Photon.Pun;

public class MenuSphere : MonoBehaviourPunCallbacks
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
