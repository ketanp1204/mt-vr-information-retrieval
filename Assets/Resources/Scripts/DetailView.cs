using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Vrsys;

public class DetailView : MonoBehaviour
{
    public InputActionProperty selectHoldAction;
    public MetaRealObject mro;
    public GameObject joinUserGO;

    public Transform playerSpawnLoc;
    public Transform objectSpawnLoc;

    private GameObject localUser;
    private GameObject dViewObjectGO;
    private GameObject userDisplay;
    private GameObject userDisplayAtOrigLoc;

    // temporary
    public InputActionProperty select2;

    private bool isHolding = false;

    // Start is called before the first frame update
    void Start()
    {
        selectHoldAction.action.performed += StartHoldAction;
        // select2.action.performed += StartHoldAction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartHoldAction(InputAction.CallbackContext context)
    {
        localUser = Vrsys.NetworkUser.localNetworkUser.gameObject;
        userDisplay = localUser.GetComponent<NetworkUser>().userDisplayGO;
        Vector3 origLoc = localUser.transform.position;
        userDisplay.transform.position = origLoc;

        localUser.transform.position = playerSpawnLoc.position;
        PhotonView photonView = localUser.GetComponent<PhotonView>();
        if(photonView.IsMine)
        {
            photonView.RPC("SyncUserDisplay", RpcTarget.All, true);
            // PhotonNetwork.Instantiate("UtilityPrefabs/JoinUser", origLoc.position, Vrsys.Utility.FindRecursive(localUser, "Main Camera").transform.localRotation, 0);
            
        }
        Vector3 objectOrigLoc = mro.gameObject.transform.position;
        mro.gameObject.transform.position = objectSpawnLoc.position;
        mro.HideDetailViewOption();
        mro.isInDetailView = true;
    }

    public void JoinDetailView()
    {
        // localUser = Vrsys.NetworkUser.localNetworkUser.gameObject;
        // localUser.transform.position = playerSpawnLoc.position;
    }
}
