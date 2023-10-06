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
    private UserDisplay uD;
    public GameObject other;

    void Start()
    {
        uD = transform.root.GetComponent<UserDisplay>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        other = args.interactorObject.transform.root.gameObject;

        StartCoroutine(JoinDV());
    }

    private IEnumerator JoinDV()
    {
        NetworkUser otherPlayer = other.GetComponent<NetworkUser>();

        // Fade the screen out 
        otherPlayer.FadeOutScreen();

        // Wait for screen fade
        yield return new WaitForSeconds(otherPlayer.GetScreenFadeDuration());

        // Show the representation of the other player in the original location
        otherPlayer.CreateUserDisplay();

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
                    GameObject exitSphere = Instantiate(exitSpherePrefab, child.transform);

                    DVAObject dVScript = exitSphere.transform.root.GetComponent<DVAObject>();

                    exitSphere.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                        (SelectEnterEventArgs args) => { FindObjectOfType<DVManager>().JoiningUserExitDVA(uD.dVAIndex, dVScript); });
                }
            }
        }

        // Set focus objects
        var focus = Vrsys.Utility.FindRecursive(Vrsys.NetworkUser.localNetworkUser.gameObject, "FocusCamera").GetComponent<FocusSwitcher>();
        focus.SetFocused(focusObjects);

        // Add DVA User Count at index
        dVManager.AddDVAUserCount(uD.dVAIndex);

        // Fade the screen in
        otherPlayer.FadeInScreen();
    }
}
