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
            photonView.RPC("UpdateInfoTextBox", RpcTarget.Others, gameObject.name, true, text);
        }
        else if (cG.alpha == 1f)
        {
            if (displayText.text == text)
            {
                cG.alpha = 0f;
                photonView.RPC("UpdateInfoTextBox", RpcTarget.Others, gameObject.name, false, null);
            }
            else
            {
                displayText.text = text;
                cG.alpha = 1f;
                photonView.RPC("UpdateInfoTextBox", RpcTarget.Others, gameObject.name, true, text);
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
}
