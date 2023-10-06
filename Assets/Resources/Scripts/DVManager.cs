using Photon.Pun;
using Photon.Realtime;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Vrsys;

public class DVManager : MonoBehaviourPunCallbacks
{
    // Public Variables
    public GameObject exitSpherePrefab;


    // Private Variables //
    [SerializeField] private List<GameObject> dVAObjects;
    [SerializeField] private List<Vector3> dVALocs;
    [SerializeField] private List<int> dVAUserCounts;
    private List<GameObject> focusObjects;
    private bool isInDetailView = false;
    private GameObject userGO;
    private Transform detailViewAreaTransform;
    private GameObject userDisplayGO;
    [SerializeField]private Vector3 dVASpawnLoc;



    // Start is called before the first frame update
    void Start()
    {
        dVASpawnLoc = Vector3.zero;
        dVAObjects = new List<GameObject>();
        dVALocs = new List<Vector3>();
        dVAUserCounts = new List<int>();
        focusObjects = new List<GameObject>();
    }

    public void CreateDVA(string itemName)
    {
        // Get user GameObject
        userGO = Vrsys.NetworkUser.localGameObject;

        // Enter detail view
        StartCoroutine(EnterDetailViewingArea(itemName));
    }
    
    public void ExitDVA(int index, DVAObject dVScript)
    {
        StartCoroutine(ExitDetailViewingArea(index, dVScript));
    }

    public void JoiningUserExitDVA(int index, DVAObject dVScript)
    {
        StartCoroutine(JUserExitDVA(index, dVScript));
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

        // Add User Count at new index
        AddNewDVAUserCount();

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

        string dVName = "DV_" + itemName;

        // Get focus objects
        focusObjects.Clear();
        foreach (Transform child in dVAObject.GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.name.Contains(itemName))
            {
                child.gameObject.SetActive(true);
                focusObjects.Add(child.gameObject);

                if (child.gameObject.name == dVName)
                {
                    GameObject exitSphere = Instantiate(exitSpherePrefab, child.transform);

                    DVAObject dVScript = dVAObject.GetComponent<DVAObject>();

                    exitSphere.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                        (SelectEnterEventArgs args) => { FindObjectOfType<DVManager>().ExitDVA(index, dVScript); });
                }
            }
        }

        // Add controller rays to focused objects list
        focusObjects.Add(((ViewingSetupHMDAnatomy)Vrsys.NetworkUser.localNetworkUser.viewingSetupAnatomy).leftController);
        focusObjects.Add(((ViewingSetupHMDAnatomy)Vrsys.NetworkUser.localNetworkUser.viewingSetupAnatomy).rightController);
        focusObjects.Add(Vrsys.NetworkUser.localNetworkUser.GetComponent<HandRayController>().hitVisualization);

        // Set focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private IEnumerator ExitDetailViewingArea(int index, DVAObject dVScript)
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

        if (dVAUserCounts[index] == 1)
        {
            // Remove DVA Object at index
            RemoveDVAObject(index);

            // Remove DVA location vector at index
            RemoveDVALoc(index);

            // Update DVA Spawn Loc
            UpdateDVASpawnLoc(dVASpawnLoc + (Vector3.one * 20f));

            dVScript.RemoveSpawnedObjects();
        }        

        // Subtract User Count at index
        SubtractDVAUserCount(index);

        // Unset focused objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        List<GameObject> n = null;
        focus.SetFocused(n);

        // Fade the screen in
        Vrsys.NetworkUser.localNetworkUser.FadeInScreen();
    }

    private IEnumerator JUserExitDVA(int index, DVAObject dVScript)
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

        if (dVAUserCounts[index] == 1)
        {
            // Remove DVA Object at index
            RemoveDVAObject(index);

            // Remove DVA location vector at index
            RemoveDVALoc(index);

            // Update DVA Spawn Loc
            UpdateDVASpawnLoc(dVASpawnLoc + (Vector3.one * 20f));

            dVScript.RemoveSpawnedObjects();
        }

        // Subtract User Count at index
        SubtractDVAUserCount(index);        
       
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


    // Update DVA Object names

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
        Destroy(dVAObjects[index]);
        dVAObjects.RemoveAt(index);
    }


    // Update DVA Spawn Location

    public void UpdateDVASpawnLoc(Vector3 updatedLoc)
    {
        photonView.RPC(nameof(UpdateDVASpawnLocRPC), RpcTarget.All, updatedLoc);
    }

    [PunRPC]
    void UpdateDVASpawnLocRPC(Vector3 updatedLoc)
    {
        dVASpawnLoc = updatedLoc;
    }


    // Update DVA Locations

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


    // Update DVA User Counts
    
    public void AddNewDVAUserCount()
    {
        photonView.RPC(nameof(AddNewDVAUserCountRPC), RpcTarget.All);
    }

    [PunRPC]
    void AddNewDVAUserCountRPC()
    {
        dVAUserCounts.Add(1);
    }
    
    public void AddDVAUserCount(int index)
    {
        photonView.RPC(nameof(AddDVAUserCountRPC), RpcTarget.All, index);
    }

    [PunRPC]
    void AddDVAUserCountRPC(int index)
    {
        dVAUserCounts[index] += 1;
    }

    public void SubtractDVAUserCount(int index)
    {
        if (dVAUserCounts[index] > 1)
        {
            Debug.Log("subtract User Count");
            photonView.RPC(nameof(SubtractDVAUserCountRPC), RpcTarget.All, index);
        }
        else
        {
            photonView.RPC(nameof(RemoveDVAUserCountRPC), RpcTarget.All, index);
        }
    }

    [PunRPC]
    void SubtractDVAUserCountRPC(int index)
    {
        dVAUserCounts[index] -= 1;
    }

    [PunRPC]
    void RemoveDVAUserCountRPC(int index)
    {
        dVAUserCounts.RemoveAt(index);
    }

    [PunRPC]
    void UpdateDVAUserCountRPC(int index, int count)
    {
        dVAUserCounts[index] = count;
    }

    [PunRPC]
    void InitDVAUserCountRPC(int count)
    {
        for (int i = 0; i < count; i++)
        {
            dVAUserCounts.Add(0);
        }
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

            photonView.RPC(nameof(UpdateDVASpawnLocRPC), newPlayer, dVASpawnLoc);

            for (int i = 0; i < dVALocs.Count; i++)
            {
                photonView.RPC(nameof(AddDVALocRPC), newPlayer, dVALocs[i]);
            }

            photonView.RPC(nameof(InitDVAUserCountRPC), newPlayer, dVAUserCounts.Count);

            for (int i = 0; i < dVAUserCounts.Count; i++)
            {
                photonView.RPC(nameof(UpdateDVAUserCountRPC), newPlayer, i, dVAUserCounts[i]);
            }
        }
    }
}
