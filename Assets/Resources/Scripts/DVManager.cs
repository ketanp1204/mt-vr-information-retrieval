using Photon.Pun;
using Photon.Realtime;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Vrsys;

public class DVManager : MonoBehaviourPunCallbacks
{    

    /* Private Variables */
    [SerializeField] private List<GameObject> dVAObjects;
    [SerializeField] private List<Vector3> dVALocs;
    private List<GameObject> focusObjects;
    private bool isInDetailView = false;
    private GameObject userGO;
    private Transform detailViewAreaTransform;
    private GameObject userDisplayGO;
    private Vector3 dVASpawnLoc;



    // Start is called before the first frame update
    void Start()
    {
        dVASpawnLoc = Vector3.zero;
        dVAObjects = new List<GameObject>();
        dVALocs = new List<Vector3>();
        focusObjects = new List<GameObject>();
    }

    public void CreateDVA(string itemName)
    {
        // Get user GameObject
        userGO = Vrsys.NetworkUser.localGameObject;

        // Enter detail view
        StartCoroutine(EnterDetailViewingArea(itemName));
    }
    
    public void ExitDVA(int index)
    {
        StartCoroutine(ExitDetailViewingArea(index));
    }

    public void JoiningUserExitDVA(int index)
    {
        StartCoroutine(JUserExitDVA(index));
    }

    private IEnumerator EnterDetailViewingArea(string itemName)
    {
        // Fade the screen out 
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

        // Initialize instantiation parameters
        int index = GetCurrentCount();
        object[] info = new object[] { index, itemName, photonView.ViewID };
        Vector3 spawnLoc = dVASpawnLoc + (Vector3.one * -20f);

        // Create detail viewing area
        GameObject dVAObject = PhotonNetwork.Instantiate("UtilityPrefabs/DVA",
                                                            spawnLoc,
                                                            Quaternion.identity,
                                                            data: info);

        // Update spawn location of DVA for further DVAs
        UpdateDVASpawnLoc(spawnLoc);

        // Save DVA location vector at new index
        AddDVALoc(spawnLoc);

        // Add DVA to DV Manager
        AddDVAObject(dVAObject.name);

        // Show the user representation in the original location
        userDisplayGO = userGO.GetComponent<NetworkUser>().CreateUserDisplay();

        // Update parameters in displayGO
        var uD = userDisplayGO.GetComponent<UserDisplay>();
        uD.SetDVAIndex(index);
        uD.SetDVAObject(dVAObject.name);
        uD.SetItemName(itemName);

        // Teleport the player to the detail viewing area
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x + dVASpawnLoc.x,
                                      player.localPosition.y + dVASpawnLoc.y,
                                      player.localPosition.z + dVASpawnLoc.z);

        // Get focus objects
        foreach (Transform child in dVAObject.GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.name.Contains(itemName))
            {
                child.gameObject.SetActive(true);
                focusObjects.Add(child.gameObject);
            }
        }

        // Set focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private IEnumerator ExitDetailViewingArea(int index)
    {
        // Fade the screen out
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

        // Teleport the player back to the original location
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x - dVALocs[index].x,
                                      player.localPosition.y - dVALocs[index].y,
                                      player.localPosition.z - dVALocs[index].z);


        // Remove the representation of the user from the original location
        userGO.GetComponent<NetworkUser>().DestroyUserDisplay();

        // Remove DVA Object at index
        RemoveDVAObject(index);

        // Remove DVA location vector at index
        RemoveDVALoc(index);

        // Update DVA Spawn Loc
        UpdateDVASpawnLoc(dVASpawnLoc + (Vector3.one * 20f));

        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private IEnumerator JUserExitDVA(int index)
    {
        // Fade the screen out
        Vrsys.NetworkUser.localNetworkUser.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(Vrsys.NetworkUser.localNetworkUser.GetScreenFadeDuration());

        // Teleport the player back to the original location
        var player = Vrsys.NetworkUser.localNetworkUser.gameObject.transform;
        player.position = new Vector3(player.localPosition.x - dVALocs[index].x,
                                      player.localPosition.y - dVALocs[index].y,
                                      player.localPosition.z - dVALocs[index].z);

        // Remove the representation of the user from the original location
        Vrsys.NetworkUser.localNetworkUser.DestroyUserDisplay();

        // Remove DVA Object at index
        RemoveDVAObject(index);

        // Remove DVA location vector at index
        RemoveDVALoc(index);

        // Update DVA Spawn Loc
        UpdateDVASpawnLoc(dVASpawnLoc + (Vector3.one * 20f));

        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private int GetCurrentCount()
    {
        return dVAObjects.Count;
    }



    public Transform GetDVATransform(int index)
    {
        return dVAObjects[index].transform;
    }


    public void AddDVAObject(string name)
    {
        photonView.RPC(nameof(AddDVAObjectRPC), RpcTarget.All, name);
    }

    [PunRPC]
    void AddDVAObjectRPC(string name)
    {
        dVAObjects.Add(GameObject.Find(name));
    }

    public void RemoveDVAObject(int index)
    {
        photonView.RPC(nameof(RemoveDVAObjectRPC), RpcTarget.All, index);
    }

    [PunRPC]
    void RemoveDVAObjectRPC(int index)
    {
        dVAObjects.RemoveAt(index);
    }



    public void UpdateDVASpawnLoc(Vector3 updatedLoc)
    {
        photonView.RPC(nameof(UpdateDVASpawnLocRPC), RpcTarget.All, updatedLoc);
    }

    [PunRPC]
    void UpdateDVASpawnLocRPC(Vector3 updatedLoc)
    {
        dVASpawnLoc = updatedLoc;
    }



    public void AddDVALoc(Vector3 loc)
    {
        photonView.RPC(nameof(AddDVALocRPC), RpcTarget.All, loc);
    }

    [PunRPC]
    void AddDVALocRPC(Vector3 loc)
    {
        dVALocs.Add(loc);
    }

    public void RemoveDVALoc(int index)
    {
        photonView.RPC(nameof(RemoveDVALocRPC), RpcTarget.All, index);
    }

    [PunRPC]
    void RemoveDVALocRPC(int index)
    {
        dVALocs.RemoveAt(index);
    }



    // Late join stuff

    [PunRPC]
    public void LateJoinDVAUpdate(string[] dvaObjects)
    {
        foreach (var name in dvaObjects)
        {
            dVAObjects.Add(GameObject.Find(name));
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var obj in dVAObjects)
            {
                photonView.RPC(nameof(AddDVAObjectRPC), newPlayer, obj.name);                
            }

            photonView.RPC(nameof(UpdateDVASpawnLoc), newPlayer, dVASpawnLoc);

            for (int i = 0; i < dVALocs.Count; i++)
            {
                photonView.RPC(nameof(AddDVALoc), newPlayer, i, dVALocs[i]);
            }
        }
    }
}
