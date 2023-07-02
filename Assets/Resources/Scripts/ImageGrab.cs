using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ImageGrab : MonoBehaviour
{
    public GameObject imageCanvasPrefab;
    public Vector2 offset;

    private bool isGrabbing = false;
    private Transform controllerTransform;
    private Transform attachPoint;
    GameObject clone;
    private RectTransform rect;

    private void Update()
    {
        if (isGrabbing)
        {
            rect.anchoredPosition3D = controllerTransform.position + (Vector3)offset;
        }
    }

    public void Grab(GameObject controller)
    {
        var canvas = Instantiate(imageCanvasPrefab, transform.position, transform.rotation);
        clone = Instantiate(gameObject, canvas.transform, true);
        rect = clone.GetComponent<RectTransform>();
        StartGrabbing(controller);

        /*
        var clone = Instantiate(gameObject, transform.parent);
        // clone.GetComponent<ImageGrab>().StartGrabbing(controller);
        // var interactable = clone.AddComponent<XRGrabInteractable>();
        var interactable = clone.GetComponent<XRGrabInteractable>();
        clone.GetComponent<BoxCollider>().enabled = true;
        */
    }

    public void XRSelectEnter(SelectEnterEventArgs selectEnterEventArgs)
    {
        
    }

    public void StartGrabbing(GameObject controller)
    {
        controllerTransform = controller.transform;
        isGrabbing = true;
    }

    public void StopGrabbing()
    {
        isGrabbing = false;
    }
}
