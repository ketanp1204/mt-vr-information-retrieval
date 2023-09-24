using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    // Public Variables //

    public PhotonView photonView;


    // Private Variables //

    private TextMeshProUGUI displayText;
    private CanvasGroup cG;



    // Start is called before the first frame update
    void Start()
    {
        cG = GetComponent<CanvasGroup>();
        displayText = transform.Find("Viewport/Content/Text").GetComponent<TextMeshProUGUI>();
    }

    public void DisplayText(string text)
    {
        if (cG.alpha == 0f)
        {
            displayText.text = text;
            cG.alpha = 1f;
            photonView.RPC(nameof(SyncTextBox), RpcTarget.Others, true, text);
        }
        else if (cG.alpha == 1f)
        {
            if (displayText.text == text)
            {
                cG.alpha = 0f;
            }
            else
            {
                displayText.text = text;
                cG.alpha = 1f;
            }
        }
    }

    public void HideTextBox()
    {
        if (cG.alpha == 1f)
        {
            cG.alpha = 0f;
        }
    }

    [PunRPC]
    void SyncTextBox(bool visibility, string text)
    {
        if (visibility)
        {
            cG.alpha = 1f;
            displayText.text = text;
        }
        else
        {
            cG.alpha = 0f;
        }
    }
}
