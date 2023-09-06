using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OwnershipTransfer : MonoBehaviourPun
{
    public void TakeOwnership()
    {
        base.photonView.RequestOwnership();
    }
}
