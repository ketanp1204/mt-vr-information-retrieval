using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Vrsys;

public class JoinDetailView : XRBaseInteractable
{
    /* Public Variables */
    public PhotonView photonView;
    public GameObject exitSpherePrefab;
    public GameObject userGO;

    /* Private Variables */
    private SphereCollider sphCol;
    private Transform detailViewAreaTransform;
    public UserDisplay uD;

    void Start()
    {
        uD = transform.root.GetComponent<UserDisplay>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        GameObject otherPlayer = args.interactorObject.transform.root.gameObject;

        // Show the representation of the other player in the original location
        otherPlayer.GetComponent<NetworkUser>().CreateUserDisplay();

        // Get the transform for the detail viewing area
        DVManager dVManager = FindObjectOfType<DVManager>();
        detailViewAreaTransform = dVManager.GetDVATransform(uD.dVAIndex);

        // Teleport the other player to the detail view area
        Transform otherPlayerTransform = otherPlayer.transform;
        otherPlayerTransform.position = new Vector3(otherPlayerTransform.localPosition.x + detailViewAreaTransform.localPosition.x,
                                                    otherPlayerTransform.localPosition.y + detailViewAreaTransform.localPosition.y,
                                                    otherPlayerTransform.localPosition.z + detailViewAreaTransform.localPosition.z);

        // Get focus objects
        List<GameObject> focusObjects = new List<GameObject>();

        string dVName = "DV_" + uD.itemName;

        foreach (Transform child in GameObject.Find(uD.dVAObject).GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.name.Contains(uD.itemName))
            {
                child.gameObject.SetActive(true);
                focusObjects.Add(child.gameObject);

                if (child.gameObject.name == dVName)
                {
                    GameObject exitSphere = GameObject.Instantiate(exitSpherePrefab, child.transform);

                    exitSphere.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                        (SelectEnterEventArgs args) => { FindObjectOfType<DVManager>().JoiningUserExitDVA(uD.dVAIndex); });
                }
            }
        }

        // Set focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);
    }
}
