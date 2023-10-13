using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class DescBoxPrefab : MonoBehaviourPunCallbacks
{

    // Public Variables //

    public TextMeshProUGUI textField;



    // Private Variables

    private string gOName = "";
    public ExhibitInformation exhibitInfo = null;
    private string exhibitNameString = "";
    private int exhibitInfoItemIndex = 0;


    public void SetText(string text)
    {
        textField.text = text;
    }

    public void SetInfoFromExhibitInfo(string exhibitName)
    {
        SetExhibitInfo(exhibitName);

        photonView.RPC(nameof(SetInfoFromExhibitInfoRPC), RpcTarget.Others);
    }

    private void SetExhibitInfo(string exhibitName)
    {
        // Get exhibit information object
        ExhibitInfoRefs exhibitInfoRefs = Resources.Load("Miscellaneous/ExhibitInfoRefs") as ExhibitInfoRefs;
        for (int i = 0; i < exhibitInfoRefs.exhibitInfos.Length; i++)
        {
            if (exhibitInfoRefs.exhibitInfos[i].exhibitName == exhibitName)
            {
                exhibitInfo = exhibitInfoRefs.exhibitInfos[i].exhibitInfo;
            }
        }

        exhibitNameString = exhibitName;
    }

    [PunRPC]
    void SetInfoFromExhibitInfoRPC()
    {
        // Set text value
        SetText(exhibitInfo.basicInfoText.text);

        // Update GameObject name
        gOName = "DB" + exhibitNameString;
        gameObject.name = gOName;
    }
    

    // Late join stuff

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetLateJoinInfo), newPlayer, exhibitNameString);
        }
    }

    [PunRPC]
    void SetLateJoinInfo(string exhibitName)
    {
        SetExhibitInfo(exhibitName);

        SetInfoFromExhibitInfoRPC();
    }
}
