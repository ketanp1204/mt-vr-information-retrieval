using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ContentSharing : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public GameObject visibilityObject;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            Debug.Log("mine");
        }
        else
        {
            Debug.Log("not mine");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
