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

    /* Private Variables */
    private SphereCollider sphCol;
    private Transform detailViewAreaTransform;
    

    // Start is called before the first frame update
    void Start()
    {
        detailViewAreaTransform = GameObject.Find("DetailViewingArea").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        GameObject player = args.interactorObject.transform.root.gameObject;
        Debug.Log(player.name);

        if (player != Vrsys.NetworkUser.localNetworkUser.gameObject)
        {
            /*
            // Create a representation of the other player in the original location
            var displayGO = player.GetComponent<NetworkUser>().userDisplayGO;
            Debug.Log(displayGO.name);
            displayGO.SetActive(true);
            displayGO.transform.SetParent(null);
            displayGO.GetComponent<UserDisplaySync>().SyncUserDisplay(true);
            */

            // Teleport the other player to the detail view area
            Transform playerTransform = player.transform;
            playerTransform.position = new Vector3(playerTransform.localPosition.x + detailViewAreaTransform.localPosition.x,
                                          playerTransform.localPosition.y,
                                          playerTransform.localPosition.z + detailViewAreaTransform.localPosition.z);
        }
    }

    [PunRPC]
    void SyncUserDisplay(string objName, bool activeState)
    {
        GameObject.Find(objName).SetActive(activeState);
    }

    public void HandleSelect()
    {

    }
}
