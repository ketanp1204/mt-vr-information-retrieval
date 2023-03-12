using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class HandControllerSwitcher : MonoBehaviourPun
{
    public InputActionProperty switchAction;

    public GameObject leftHandGO;
    public GameObject rightHandGO;

    public GameObject leftControllerGO;
    public GameObject rightControllerGO;

    [SerializeField]
    private bool handsActive = false;
    private bool isLocal = false;


    // Start is called before the first frame update
    void Start()
    {
        if (photonView != null &&  photonView.IsMine)
        {
            isLocal = true;
            if (handsActive)
            {
                ActivateHands();
            }
            else
            {
                ActivateControllers();
            }
        } else if (photonView == null) // version without photonView
        {
            isLocal = true;
            if (handsActive)
            {
                ActivateHands();
            }
            else
            {
                ActivateControllers();
            }
        }
        else
        {
            isLocal = false;
            ActivateHands();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocal)
        {
            if (switchAction.action.WasPressedThisFrame())
            {
                handsActive = !handsActive;
                if (handsActive)
                {
                    ActivateHands();
                } else
                {
                    ActivateControllers();
                }
            }
        }
    }

    private void ActivateHands()
    {
        leftHandGO.SetActive(true);
        rightHandGO.SetActive(true);
        leftControllerGO.SetActive(false);
        rightControllerGO.SetActive(false);
    }

    private void ActivateControllers()
    {
        leftHandGO.SetActive(false);
        rightHandGO.SetActive(false);
        leftControllerGO.SetActive(true);
        rightControllerGO.SetActive(true);
    }
}
