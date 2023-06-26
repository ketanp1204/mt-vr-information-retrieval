using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Vrsys;

public class DetailViewManager : MonoBehaviourPunCallbacks
{
    /* Public Variables */
    public List<GameObject> detailViewingAreaGOs;
    public string itemName;

    /* Private Variables */
    [SerializeField] private List<GameObject> focusObjects;
    private bool isInDetailView = false;
    private GameObject userGO;
    private Transform detailViewAreaTransform;
    private GameObject userDisplayGO;
    private Vector3 detailViewingAreaSpawnLoc;




    // Start is called before the first frame update
    void Start()
    {
        detailViewingAreaSpawnLoc = Vector3.zero;
        detailViewingAreaGOs = new List<GameObject>();
        focusObjects = new List<GameObject>();
    }

    public void HandleSelect()
    {
        if (!isInDetailView)
        {
            // Get user GameObject
            userGO = Vrsys.NetworkUser.localNetworkUser.gameObject;

            // Enter detail view
            StartCoroutine(EnterDetailViewingArea());

            isInDetailView = true;
        }
        else
        {
            // Exit detail view
            StartCoroutine(ExitDetailViewingArea());

            isInDetailView = false;
        }
    }

    private IEnumerator EnterDetailViewingArea()
    {
        // Fade the screen out 
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

        // Initialize instantiation parameters
        int index = GetCurrentCount();

        object[] info = new object[] { index, itemName, photonView.ViewID };

        // Create detail viewing area
        GameObject dVAObject = PhotonNetwork.Instantiate("UtilityPrefabs/DVA",
                                                            detailViewingAreaSpawnLoc + new Vector3(-20f, -20f, -20f),
                                                            Quaternion.identity,
                                                            data: info);

        // Update spawn location of DVA for further DVAs
        UpdateDVASpawnLocWrapper(new Vector3(-20f, -20f, -20f));

        // Add DVA to DV Manager
        AddDVAObjectWrapper(dVAObject.name);

        // Show the representation of the player in the original location and pass the viewID of this photonView
        userDisplayGO = userGO.GetComponent<NetworkUser>().CreateUserDisplay();

        // Show the transparent sphere near the representation
        // GetComponent<MenuArea>().SetMenuSphereVisibility(true);

        // Update parameters in displayGO
        userDisplayGO.GetComponent<UserDisplay>().SetDVAIndexWrapper(index);
        userDisplayGO.GetComponent<UserDisplay>().SetDVManagerWrapper(photonView.ViewID);

        // Teleport the player to the detail viewing area
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x + detailViewingAreaSpawnLoc.x,
                                      player.localPosition.y + detailViewingAreaSpawnLoc.y,
                                      player.localPosition.z + detailViewingAreaSpawnLoc.z);

        // Get focus objects
        foreach (Transform child in dVAObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name.Contains(itemName))
            {
                focusObjects.Add(child.gameObject);
            }
        }

        // Set focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private IEnumerator ExitDetailViewingArea()
    {
        // Fade the screen out
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

        // Teleport the player back to the original location
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x - detailViewingAreaSpawnLoc.x,
                                      player.localPosition.y - detailViewingAreaSpawnLoc.y,
                                      player.localPosition.z - detailViewingAreaSpawnLoc.z);


        // Remove the representation of the user from the original location
        userGO.GetComponent<NetworkUser>().DestroyUserDisplay();

        /*
        // Remove the representation of the user from the original location
        displayGO.transform.SetParent(userGO.transform);
        displayGO.GetComponent<UserDisplaySync>().SyncUserDisplay(false); 
        displayGO.SetActive(false);
        */

        // Hide the transparent sphere near the representation
        //GetComponent<MenuArea>().SetMenuSphereVisibility(false);

        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    

    public int GetCurrentCount()
    {
        return detailViewingAreaGOs.Count;
    }

    // Returns the index of the detail viewing area object created
    public void AddDVAObjectWrapper(string name)
    {
        photonView.RPC(nameof(AddDVAObject), RpcTarget.All, name);
    }

    [PunRPC]
    void AddDVAObject(string name)
    {
        detailViewingAreaGOs.Add(GameObject.Find(name));
    }

    public void UpdateDVASpawnLocWrapper(Vector3 updateVector)
    {
        photonView.RPC(nameof(UpdateDVASpawnLoc), RpcTarget.AllBuffered, updateVector);
    }

    [PunRPC]
    void UpdateDVASpawnLoc(Vector3 updateVector)
    {
        detailViewingAreaSpawnLoc += updateVector;
    }

    public Transform GetDVATransform(int index)
    {
        return detailViewingAreaGOs[index].transform;
    }
    
    // Late join stuff

    [PunRPC]
    public void LateJoinDVAUpdate(string[] dvaObjects)
    {
        foreach (var name in dvaObjects)
        {
            detailViewingAreaGOs.Add(GameObject.Find(name));     
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var obj in detailViewingAreaGOs)
            {
                photonView.RPC(nameof(AddDVAObject), newPlayer, obj.name);
            }
        }
    }
}
