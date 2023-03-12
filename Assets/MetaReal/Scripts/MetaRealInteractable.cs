using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MetaRealInteractable : XRBaseInteractable
{
    public MetaRealObject mro;
    public bool hideCollider = true;

    private DetailView detailView;

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
        if (!mro.isInDetailView)
        {
            mro.ShowLabel();
        }
        
        //XRBaseInteractor interactor = (XRBaseInteractor)args.interactorObject;
        //MetaRealController controller = GetCreativeControllerFromInteractor(interactor);
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        if(!mro.isInDetailView)
        {
            mro.HideLabel();
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (gameObject.name == "JoinUserCollider")
        {
            detailView.JoinDetailView();
        }
        else
        {
            mro.HideLabel();
            mro.ShowDetailViewOption();
            mro.ShowBasicInfo();

            detailView = mro.detailViewOptionGO.GetComponent<DetailView>();

            if (detailView != null)
            {
                // detailView.StartHoldAction();
            }
        }
        
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {

    }
}
