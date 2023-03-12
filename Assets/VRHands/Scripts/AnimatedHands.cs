using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
public class AnimatedHands : MonoBehaviourPunCallbacks, IPunObservable
{
    public InputActionProperty trigger;
    public InputActionProperty grip;

    public float speed = 10f;
    Animator animator;

    private float gripTarget;
    private float triggerTarget;
    private float gripCurrent;
    private float triggerCurrent;

    private string animatorGripParam = "Grip";
    private string animatorTriggerParam = "Trigger";

    private bool isLocal;
    private bool isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView != null && photonView.IsMine)
        {
            isLocal = true;
        }
        else if (photonView == null) // version without photonView
        {
            isLocal = true;
        }
        else
        {
            isLocal = false;
        }

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocal)
        {
            gripTarget = grip.action.ReadValue<float>();
            
            triggerTarget = trigger.action.ReadValue<float>();

            AnimateHand();
        }
    }

    void AnimateHand()
    {
        if (gripCurrent != gripTarget)
        {
            gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * speed);
            
            animator.SetFloat(animatorGripParam, gripCurrent);
        }

        if (triggerCurrent != triggerTarget)
        {
            triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, Time.deltaTime * speed);
            animator.SetFloat(animatorTriggerParam, triggerCurrent);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && isLocal)
        {
            stream.SendNext(gripCurrent);
            stream.SendNext(triggerCurrent);
        }
        else if (stream.IsReading)
        {
            gripTarget = (float)stream.ReceiveNext();
            triggerTarget = (float)stream.ReceiveNext();

            AnimateHand();
        }
    }
}
