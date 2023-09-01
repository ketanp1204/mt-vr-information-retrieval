using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class testDV : MonoBehaviour
{
    public DVManager DVManager;
    public JoinDetailView joinDetailView;
    public XRBaseInteractor interactorObject;

    public bool createSession = false;
    public bool joinSession = false;


    private void Update()
    {
        if (createSession)
        {
            createSession = false;

            DVManager.CreateDVA("GS_B02");
        }

        if (joinSession)
        {
            joinSession = false;

            SelectEnterEventArgs args = new SelectEnterEventArgs();
            args.interactorObject = interactorObject;

            joinDetailView.GetComponent<XRBaseInteractable>().selectEntered.Invoke(args);
        }
    }
}
