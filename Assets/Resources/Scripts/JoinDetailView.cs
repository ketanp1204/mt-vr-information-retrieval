using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Vrsys;

public class JoinDetailView : XRBaseInteractable
{
    /* Public Variables */
    public PhotonView photonView;
    

    public GameObject userGO;

    /* Private Variables */
    private SphereCollider sphCol;
    private Transform detailViewAreaTransform;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        GameObject otherPlayer = args.interactorObject.transform.root.gameObject;

        // Show the representation of the other player in the original location
        otherPlayer.GetComponent<NetworkUser>().CreateUserDisplay();

        // Get the transform for the detail viewing area
        detailViewAreaTransform.localPosition = transform.root.GetComponent<UserDisplay>().detailViewingAreaPosition;

        // Update the interest group that the other player subscribes to
        int iG = transform.root.GetComponent<UserDisplay>().interestGroup;
        otherPlayer.GetComponent<NetworkUser>().UpdateInterestGroup(iG);

        // Teleport the other player to the detail view area
        Transform otherPlayerTransform = otherPlayer.transform;
        otherPlayerTransform.position = new Vector3(otherPlayerTransform.localPosition.x + detailViewAreaTransform.localPosition.x,
                                                    otherPlayerTransform.localPosition.y + detailViewAreaTransform.localPosition.y,
                                                    otherPlayerTransform.localPosition.z + detailViewAreaTransform.localPosition.z);
    }
}
