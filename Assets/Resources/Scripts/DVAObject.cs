using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DVAObject : MonoBehaviour, IPunInstantiateMagicCallback
{
    public GameObject exitSpherePrefab;

    private string dVName;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        int index = (int)data[0];
        gameObject.name = "DVA" + index.ToString();

        string itemName = (string)data[1];

        dVName = "DV_" + itemName;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.name == dVName)
            {
                GameObject exitSphere = GameObject.Instantiate(exitSpherePrefab, child.transform);

                exitSphere.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                    (SelectEnterEventArgs args) => { FindObjectOfType<DVManager>().ExitDVA(index); });

                // child.transform.Find("ExitSphere").GetComponent<XRSimpleInteractable>().selectEntered.AddListener(
                                    //(SelectEnterEventArgs args) => { FindObjectOfType<DVManager>().ExitDVA(index); });
            }
        }
    }
}
