using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuAction : MonoBehaviour
{
    /* Public Variables */

    public UnityEvent actionHoverEnteredEvents;
    public UnityEvent actionHoverExitedEvents;

    /* Private Variables */
    // All child actions of this action
    [SerializeField] private List<GameObject> childActions = new List<GameObject>();

    // All other actions in the same hierarchy
    [SerializeField] private List<GameObject> otherActions = new List<GameObject>();

    // Interaction animation duration 
    [SerializeField, Range(0f, 0.3f)] private float animDuration = 0.1f;

    // Radius of the circle arrangement of the child spheres
    [SerializeField] private float circleRadius = 2f;

    private bool isSelected = false;
    private GameObject parent;
    private Vector3 actionPosition; 

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
        StartCoroutine(TranslateToZeroPosition());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleHoverEntered()
    {
        if (!isSelected)
        {
            actionHoverEnteredEvents.Invoke();
        }
    }

    public void HandleHoverExited()
    {
        if (!isSelected)
        {
            actionHoverExitedEvents.Invoke();
        }
    }

    public void HandleMenuSelect()
    {
        if (!isSelected)
        {
            // Get current localPosition
            actionPosition = transform.localPosition;

            isSelected = true;

            // Hide the other menu items and the main menu sphere
            StartCoroutine(FadeOutParentAndOtherActions());

            // Reposition this menu item to zero local position after a delay
            StartCoroutine(TranslateToZeroPosition());

            // Animate children of this menu item to their circular arrangement positions after delay
            StartCoroutine(AnimateChildrenToCircle());

        }
        else
        {
            isSelected = false;

            // Show the other menu items and the main menu sphere after a delay
            StartCoroutine(FadeInParentAndOtherActions());

            // Reposition this menu item to its original position
            StartCoroutine(TranslateToFinalPosition());

            // Animate children of this menu item to their zero positions
            StartCoroutine(AnimateChildrenToZero());
        }
    }

    public void HandleActionSelect()
    {
        if (!isSelected)
        {
            // Get current localPosition
            actionPosition = transform.localPosition;

            isSelected = true;

            // Hide hover preview if exists
            if (actionHoverExitedEvents.GetPersistentEventCount() > 0)
                actionHoverExitedEvents.Invoke();

            // Hide other actions and the parent sphere
            StartCoroutine(FadeOutParentAndOtherActions());

            // Reposition this action to zero local position after a delay
            StartCoroutine(TranslateToZeroPosition());

            // Set child object active after delay
            StartCoroutine(SetChildActiveAfterDelay());
        }
        else
        {
            isSelected = false;

            // Show the other actions and the parent sphere after a delay
            StartCoroutine(FadeInParentAndOtherActions());

            // Reposition this action to its original position
            StartCoroutine(TranslateToFinalPosition());

            // Set child object inactive
            childActions[0].SetActive(false);

        }
    }

    private IEnumerator FadeOutParentAndOtherActions()
    {
        // Get parent mesh renderer 
        Renderer rend = parent.GetComponent<MeshRenderer>();

        // Disable parent collider
        parent.GetComponent<SphereCollider>().enabled = false;

        // Hide parent display text
        GameObject displayText = parent.GetComponentInChildren<TextMeshPro>(true).gameObject;
        displayText.SetActive(false);

        float t = 0f;
        while (t < animDuration)
        {
            // Animate parent transparency
            Color c = rend.material.color;
            c.a = Mathf.Lerp(1f, 0f, t / animDuration);
            rend.material.color = c;

            // Animate other actions transparency
            foreach (var action in otherActions)
            {
                Color col = action.GetComponent<MeshRenderer>().material.color;
                col.a = Mathf.Lerp(1f, 0f, t / animDuration);
                action.GetComponent<MeshRenderer>().material.color = c;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Disable parent mesh renderer
        rend.enabled = false;

        // Disable other actions gameObjects
        foreach (var action in otherActions)
        {
            action.SetActive(false);
        }
    }

    private IEnumerator FadeInParentAndOtherActions()
    {
        // Delay
        yield return new WaitForSeconds(animDuration);

        // Get and enable parent mesh renderer 
        Renderer rend = parent.GetComponent<MeshRenderer>();
        rend.enabled = true;
        
        // Enable other actions gameObjects
        foreach (var action in otherActions)
        {
            action.SetActive(true);
        }

        float t = 0f;
        while (t < animDuration)
        {
            // Animate parent transparency
            Color c = rend.material.color;
            c.a = Mathf.Lerp(0f, 1f, t / animDuration);
            rend.material.color = c;

            // Animate other actions transparency
            foreach (var action in otherActions)
            {
                c = action.GetComponent<MeshRenderer>().material.color;
                c.a = Mathf.Lerp(0f, 1f, t / animDuration);
                action.GetComponent<MeshRenderer>().material.color = c;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Enable parent collider
        parent.GetComponent<SphereCollider>().enabled = true;

        // Show parent display text
        GameObject displayText = parent.GetComponentInChildren<TextMeshPro>(true).gameObject;
        displayText.SetActive(true);
    }

    private IEnumerator TranslateToZeroPosition()
    {
        // Delay
        yield return new WaitForSeconds(animDuration);

        float t = 0f;
        while (t < animDuration)
        {
            transform.localPosition = Vector3.Lerp(actionPosition, Vector3.zero, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }

    private IEnumerator TranslateToFinalPosition()
    {
        float t = 0f;
        while (t < animDuration)
        {
            transform.localPosition = Vector3.Lerp(Vector3.zero, actionPosition, t / animDuration);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = actionPosition;
    }

    private IEnumerator AnimateChildrenToCircle()
    {
        yield return new WaitForSeconds(animDuration);

        for (int i = 0; i < childActions.Count; i++)
        {
            // Get start and end positions
            Vector3 startPos = childActions[i].transform.localPosition;
            Vector3 endPos = CalculateChildPosition(i, childActions.Count, circleRadius);

            // Get and enable Mesh Renderer
            MeshRenderer rend = childActions[i].GetComponent<MeshRenderer>();
            rend.enabled = true;

            // Get material color
            Color c = rend.material.color;

            float t = 0f;
            while (t < animDuration)
            {
                // Animate position
                childActions[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / animDuration);

                // Animate transparency
                c.a = Mathf.Lerp(0f, 1f, t / animDuration);
                rend.material.color = c;

                t += Time.deltaTime;
                yield return null;
            }

            // Set final position
            childActions[i].transform.localPosition = endPos;

            // Set final transparency
            c.a = 1f;
            rend.material.color = c;

            // Enable child collider
            childActions[i].GetComponent<SphereCollider>().enabled = true;

            // Show display text
            GameObject displayText = childActions[i].GetComponentInChildren<TextMeshPro>(true).gameObject;
            displayText.SetActive(true);
        }
    }

    private IEnumerator AnimateChildrenToZero()
    {
        for (int i = 0; i < childActions.Count; i++)
        {
            // Get start and end positions
            Vector3 startPos = CalculateChildPosition(i, childActions.Count, circleRadius);
            Vector3 endPos = Vector3.zero;

            // Get Mesh Renderer
            MeshRenderer rend = childActions[i].GetComponent<MeshRenderer>();

            // Get material color
            Color c = rend.material.color;

            // Disable child collider
            childActions[i].GetComponent<SphereCollider>().enabled = false;

            // Hide display text
            GameObject displayText = childActions[i].GetComponentInChildren<TextMeshPro>(true).gameObject;
            displayText.SetActive(false);

            float t = 0f;
            while (t < animDuration)
            {
                // Animate position
                childActions[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / animDuration);

                // Animate transparency
                c.a = Mathf.Lerp(1f, 0f, t / animDuration);
                rend.material.color = c;

                t += Time.deltaTime;
                yield return null;
            }

            // Set final position
            childActions[i].transform.localPosition = endPos;

            // Set final transparency
            c.a = 0f;
            rend.material.color = c;

            // Disable Mesh Renderer
            rend.enabled = false;
        }
    }

    private Vector3 CalculateChildPosition(int childIndex, int totalChildren, float radius)
    {
        float angle = (float)childIndex / (float)totalChildren * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        return new Vector3(x, y, 0f);
    }

    private IEnumerator SetChildActiveAfterDelay()
    {
        yield return new WaitForSeconds(animDuration);

        childActions[0].SetActive(true);
    }


}
