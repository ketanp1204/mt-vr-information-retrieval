
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Unity.VisualScripting;

public class MenuSphere : MonoBehaviour
{
    /* Public Variables */
    [SerializeField] public List<GameObject> menuActionSpheres = new List<GameObject>();


    /* Private Variables */
    [SerializeField] private float animDuration = 0.1f;                 // Duration of the animation of the child spheres
    [SerializeField] private float circleRadius = 2f;                   // Radius of the circle arrangement of the child spheres
    [SerializeField] private float fadeDuration = 0.4f;                 // Duration of the fade animation of the menu sphere
    [SerializeField] private float fadeOutDelay = 1f;                   // Delay after which menu sphere disappears after player leaves the area
    private GameObject hoveredChildSphere;
    private GameObject currentHoverLine;
    private bool isSelected = false;
    private bool faceCameraSet = false;

    

    private BoxCollider boxCol;
    private SphereCollider sphCol;
    private Renderer meshRend;

    private void Start()
    {
        boxCol = GetComponent<BoxCollider>();
        sphCol = GetComponent<SphereCollider>();
        meshRend = GetComponent<MeshRenderer>();
        
        // Hide the menu sphere on start
        // Color col = meshRend.material.color;
        // col.a = 0f;
        // meshRend.material.SetColor("_Color", col);

        

        // Hide action spheres at start
        foreach (GameObject menuAction in menuActionSpheres)
        {
            // Make them transparent
            Color c = menuAction.GetComponent<MeshRenderer>().material.color;
            c.a = 0f;
            menuAction.GetComponent<MeshRenderer>().material.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if object is player
        if (other.gameObject == Vrsys.NetworkUser.localNetworkUser.gameObject)
        {
            // Show the menu sphere
            // StartCoroutine(ToggleVisibility(true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if object is player
        if (other.gameObject == Vrsys.NetworkUser.localNetworkUser.gameObject)
        {
            // Hide the menu sphere after delay
            // StartCoroutine(ToggleVisibility(false));
        }
    }

    private IEnumerator ToggleVisibility(bool isEntering)
    {
        if (isEntering)
        {
            Debug.Log("player enter");
            Color sphColor = meshRend.material.color;
            Color c;

            //meshRend.enabled = true;

            float timer = 0f;
            while (timer <= fadeDuration)
            {
                c = sphColor;
                c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);

                meshRend.material.SetColor("_Color", c);

                timer += Time.deltaTime;
                yield return null;
            }

            sphCol.enabled = true;
            c = sphColor;
            c.a = 1f;
            meshRend.material.SetColor("_Color", c);
        }
        else
        {
            Debug.Log("Player exit");
            yield return new WaitForSeconds(fadeOutDelay);

            Color sphColor = meshRend.material.color;
            Color c;

            float timer = 0f;
            while (timer <= fadeDuration)
            {
                c = sphColor;
                c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);

                meshRend.material.color = c;

                timer += Time.deltaTime;
                yield return null;
            }

            sphCol.enabled = false;
            //meshRend.enabled = false;
        }
    }

    public void SetFaceCamera()
    {
        if (!faceCameraSet)
        {
            FaceCamera faceCamera = GetComponent<FaceCamera>();
            if (faceCamera != null && !faceCamera.isInitialized)
            {
                faceCamera.cam = Vrsys.NetworkUser.localNetworkUser.GetCamera().gameObject;
                faceCamera.isInitialized = true;
            }
        }
    }

    public void HandleSelect()
    {
        if (!isSelected)
        {
            StartCoroutine(AnimateChildrenToCircle());
            isSelected = true;
        }
        else
        {
            StartCoroutine(AnimateChildrenToZero());
            isSelected = false;
        }
    }

    private IEnumerator AnimateChildrenToCircle()
    {
        for (int i = 0; i < menuActionSpheres.Count; i++)
        {
            // Get start and end positions
            Vector3 startPos = menuActionSpheres[i].transform.localPosition;
            Vector3 endPos = CalculateChildPosition(i, menuActionSpheres.Count, circleRadius);

            // Get and enable Mesh Renderer
            MeshRenderer rend = menuActionSpheres[i].GetComponent<MeshRenderer>();
            rend.enabled = true;

            // Get material color
            Color c = rend.material.color;

            float t = 0f;
            while (t < animDuration)     
            {
                // Animate position
                menuActionSpheres[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / animDuration);

                // Animate transparency
                c.a = Mathf.Lerp(0f, 1f, t / animDuration);
                rend.material.color = c;

                t += Time.deltaTime;
                yield return null;
            }

            // Set final position
            menuActionSpheres[i].transform.localPosition = endPos;

            // Set final transparency
            c.a = 1f;
            rend.material.color = c;

            // Enable child collider
            menuActionSpheres[i].GetComponent<SphereCollider>().enabled = true;

            // Show display text
            GameObject displayText = menuActionSpheres[i].GetComponentInChildren<TextMeshPro>(true).gameObject;
            displayText.SetActive(true);
        }
    }

    private IEnumerator AnimateChildrenToZero()
    {
        for (int i = 0; i < menuActionSpheres.Count; i++)
        {
            // Get start and end positions
            Vector3 startPos = CalculateChildPosition(i, menuActionSpheres.Count, circleRadius);
            Vector3 endPos = Vector3.zero;

            // Get Mesh Renderer
            MeshRenderer rend = menuActionSpheres[i].GetComponent<MeshRenderer>();

            // Get material color
            Color c = rend.material.color;

            // Disable child collider
            menuActionSpheres[i].GetComponent<SphereCollider>().enabled = false;

            // Hide display text
            GameObject displayText = menuActionSpheres[i].GetComponentInChildren<TextMeshPro>(true).gameObject;
            displayText.SetActive(false);

            float t = 0f;
            while (t < animDuration)
            {
                // Animate position
                menuActionSpheres[i].transform.localPosition = Vector3.Lerp(startPos, endPos, t / animDuration);

                // Animate transparency
                c.a = Mathf.Lerp(1f, 0f, t / animDuration);
                rend.material.color = c;

                t += Time.deltaTime;
                yield return null;
            }

            // Set final position
            menuActionSpheres[i].transform.localPosition = endPos;

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
}
