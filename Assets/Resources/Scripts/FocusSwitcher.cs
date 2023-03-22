using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusSwitcher : MonoBehaviour
{
    public string FocusedLayer = "Focused";

    private GameObject currentlyFocused;
    private List<GameObject> currentlyFocusedList;
    private int previousLayer;

    public void SetFocused(GameObject obj)
    {
        // enables this camera and the postProcessingVolume which is the child
        gameObject.SetActive(true);

        // if something else was focused before reset it
        if (currentlyFocused) currentlyFocused.layer = previousLayer;

        // store and focus the new object
        currentlyFocused = obj;

        if (currentlyFocused)
        {
            previousLayer = currentlyFocused.layer;
            currentlyFocused.layer = LayerMask.NameToLayer(FocusedLayer);
        }
        else
        {
            // if no object is focused disable the FocusCamera
            // and PostProcessingVolume for not wasting rendering resources
            gameObject.SetActive(false);
        }
    }

    public void SetFocused(List<GameObject> objs)
    {
        // enables this camera and the postProcessingVolume which is the child
        gameObject.SetActive(true);

        // if something else was focused before reset it
        if (currentlyFocusedList != null)
        {
            foreach (GameObject obj in currentlyFocusedList)
            {
                obj.layer = previousLayer;
                SetLayerRecursively(obj, previousLayer);
            }

            currentlyFocusedList = new List<GameObject>();
        }

        // store and focus the new object
        currentlyFocusedList = objs;

        if (currentlyFocusedList != null)
        {
            foreach (GameObject obj in currentlyFocusedList)
            {
                previousLayer = obj.layer;
                int layer = LayerMask.NameToLayer(FocusedLayer);
                obj.layer = layer;
                SetLayerRecursively(obj, layer);
            }
        }
        else
        {
            // if no object is focused disable the FocusCamera
            // and PostProcessingVolume for not wasting rendering resources
            gameObject.SetActive(false);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null)
            return;

        foreach (Transform child in obj.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = layer;
        }
    }

    // On disable make sure to reset the current object
    private void OnDisable()
    {
        if (currentlyFocused) currentlyFocused.layer = previousLayer;

        currentlyFocused = null;
    }
}
