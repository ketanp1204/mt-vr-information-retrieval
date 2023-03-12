using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
public class AnimatedControllers: MonoBehaviourPunCallbacks, IPunObservable
{
    public InputActionProperty trigger;
    public InputActionProperty grip;
    public InputActionProperty buttonA;
    public InputActionProperty buttonB;
    public InputActionProperty buttonC;
    public InputActionProperty thumbstick;

    public float speed = 10f;
    Animator animator;

    private float gripTarget;
    private float gripCurrent;

    private float triggerTarget;
    private float triggerCurrent;
    
    private float buttonATarget;
    private float buttonACurrent;

    private float buttonBTarget;
    private float buttonBCurrent;
    
    private float buttonCTarget;
    private float buttonCCurrent;

    private Vector2 thumbstickTarget;
    private Vector2 thumbstickCurrent;

    private string animatorButtonAParam = "Button 1";
    private string animatorButtonBParam = "Button 2";
    private string animatorButtonCParam = "Button 3";
    private string animatorThumbstickXParam = "Joy X";
    private string animatorThumbstickYParam = "Joy Y";
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
            buttonATarget = buttonA.action.ReadValue<float>();
            buttonBTarget = buttonB.action.ReadValue<float>();
            buttonCTarget = buttonC.action.ReadValue<float>();

            gripTarget = grip.action.ReadValue<float>();
            triggerTarget = trigger.action.ReadValue<float>();
            
            thumbstickTarget = thumbstick.action.ReadValue<Vector2>();

            AnimateController();
        }
    }

    void AnimateController()
    {
        if (buttonACurrent != buttonATarget)
        {
            buttonACurrent = Mathf.MoveTowards(buttonACurrent, buttonATarget, Time.deltaTime * speed);
            animator.SetFloat(animatorButtonAParam, buttonACurrent);
        }

        if (buttonBCurrent != buttonBTarget)
        {
            buttonBCurrent = Mathf.MoveTowards(buttonBCurrent, buttonBTarget, Time.deltaTime * speed);
            animator.SetFloat(animatorButtonBParam, buttonBCurrent);
        }

        if (buttonCCurrent != buttonCTarget)
        {
            buttonCCurrent = Mathf.MoveTowards(buttonCCurrent, buttonCTarget, Time.deltaTime * speed);
            animator.SetFloat(animatorButtonCParam, buttonCCurrent);
        }

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
        
        if (thumbstickCurrent != thumbstickTarget)
        {
            thumbstickCurrent.x = Mathf.MoveTowards(thumbstickCurrent.x, thumbstickTarget.x, Time.deltaTime * speed);
            thumbstickCurrent.y = Mathf.MoveTowards(thumbstickCurrent.y, thumbstickTarget.y, Time.deltaTime * speed);
            animator.SetFloat(animatorThumbstickXParam, thumbstickCurrent.x);
            animator.SetFloat(animatorThumbstickYParam, thumbstickCurrent.y);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && isLocal)
        {
            stream.SendNext(buttonATarget);
            stream.SendNext(buttonBTarget);
            stream.SendNext(buttonCTarget);
            stream.SendNext(gripTarget);
            stream.SendNext(triggerTarget);
            stream.SendNext(thumbstickTarget);
        }
        else if (stream.IsReading)
        {
            buttonATarget = (float)stream.ReceiveNext();
            buttonBTarget = (float)stream.ReceiveNext();
            buttonCTarget = (float)stream.ReceiveNext();
            gripTarget = (float)stream.ReceiveNext();
            triggerCurrent = (float)stream.ReceiveNext();
            thumbstickTarget = (Vector2)stream.ReceiveNext();

            AnimateController();
        }
    }
}
