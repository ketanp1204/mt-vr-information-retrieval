using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MetaRealInteractable : XRBaseInteractable
{
    public MetaRealObject mro;
    public bool hideCollider = true;


    protected override void Awake()
    {
        base.Awake();
        selectMode = InteractableSelectMode.Multiple;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mro == null)
        {
            Destroy(this);
        }
        if (hideCollider)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        //XRBaseInteractor interactor = (XRBaseInteractor)args.interactorObject;
        //MetaRealController controller = GetCreativeControllerFromInteractor(interactor);
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {

    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (gameObject.name == "JoinUserCollider")
        {

        }
        else
        {
            mro.HideLabel();
            mro.ShowDetailViewOption();
            mro.ShowBasicInfo();

        }
        
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {

    }
}
