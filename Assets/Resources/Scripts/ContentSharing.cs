using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ContentSharing : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Public Variables

    public GameObject visibilityObject;



    // Private Variables

    private GameObject contentSphere;
    private bool isSharable = false;

    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int sphereViewID = (int)data[0];

        contentSphere = PhotonView.Find(sphereViewID).transform.Find("Sphere").gameObject;

        // Hide the content for others
        if (!photonView.IsMine)
        {
            visibilityObject.SetActive(false);
        }
    }

    public void SetSharable(bool sharable)
    {
        StartCoroutine(SetSharableAfterDelay(sharable));
    }

    private IEnumerator SetSharableAfterDelay(bool sharable)
    {
        yield return new WaitForSeconds(0.1f);

        photonView.RPC(nameof(SetSharableRPC), RpcTarget.All, sharable);
    }

    [PunRPC]
    void SetSharableRPC(bool sharable)
    {
        isSharable = sharable;
    }

    private bool IsPointWithinCollider(Collider collider, Vector3 point) 
    { 
        return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon; 
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == contentSphere && isSharable)
        {
            Debug.Log(Vector3.Distance(contentSphere.transform.position, transform.position));
            isSharable = false;
            photonView.RPC(nameof(SetVisibilityRPC), RpcTarget.All, true);
            Debug.Log("visible to all");
        }
    }

    [PunRPC]
    void SetVisibilityRPC(bool visible)
    {
        visibilityObject.SetActive(visible);
    }
}
