using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ImageGrab : MonoBehaviour
{
    private bool isGrabbing = false;
    private Transform controllerTransform;
    private Vector3 offset;

    private void Update()
    {
        if (isGrabbing)
        {
            transform.position = controllerTransform.position;
        }
    }

    public void Grab(GameObject controller)
    {
        var clone = Instantiate(gameObject, transform.parent);
        // clone.GetComponent<ImageGrab>().StartGrabbing(controller);
        var interactable = clone.AddComponent<XROffsetGrabInteractable>();
        FindObjectOfType<XRInteractionManager>().SelectEnter((IXRSelectInteractor)controller.GetComponent<XRDirectInteractor>(), 
            (IXRSelectInteractable)interactable);

        // var rect = Instantiate(gameObject, controller.transform.Find("ImageCanvas")).GetComponent<RectTransform>();
        // rect.localRotation = Quaternion.Euler(0f, 180f, 0f);
        // rect.anchoredPosition3D = Vector3.zero;
    }

    public void StartGrabbing(GameObject controller)
    {
        controllerTransform = controller.transform;
        isGrabbing = true;
        offset = controllerTransform.position - transform.position;
    }

    public void StopGrabbing()
    {
        isGrabbing = false;
    }
}
