
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class MenuSphere : MonoBehaviour
{
    /* Public Variables */
    public GameObject linePrefab;
    [SerializeField] public List<GameObject> menuActionSpheres = new List<GameObject>();


    /* Private Variables */
    [SerializeField] private float animDuration = 0.1f;                 // Duration of the animation of the child spheres
    [SerializeField] private float circleRadius = 2f;                   // Radius of the circle arrangement of the child spheres
    private GameObject hoveredChildSphere;
    private GameObject currentHoverLine;
    private bool isSelected = false;
    private bool faceCameraSet = false;



    private void Start()
    {
        // Hide action spheres at start
        foreach (GameObject menuAction in menuActionSpheres)
        {
            // Make them transparent
            Color c = menuAction.GetComponent<MeshRenderer>().material.color;
            c.a = 0f;
            menuAction.GetComponent<MeshRenderer>().material.color = c;
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

    public void HandleSelectEntered()
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

    public void HandleSelectExited()
    {
        if (hoveredChildSphere != null)
        {

        }
    }

    public void SetHoveredChildSphere(GameObject childSphere)
    {
        hoveredChildSphere = childSphere;

        /*
        // Instantiate line prefab
        GameObject line = Instantiate(linePrefab);
        currentHoverLine = line;

        // Set line positions and scale
        line.transform.parent = transform;
        line.transform.localPosition = Vector3.zero;
        line.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(transform.position, hoveredChildSphere.transform.position));
        line.transform.LookAt(hoveredChildSphere.transform);
        line.transform.position = (transform.position + hoveredChildSphere.transform.position) / 2;
        Vector3 direction = Vector3.Normalize(hoveredChildSphere.transform.position - line.transform.position);
        line.transform.localRotation = Quaternion.LookRotation(direction, line.transform.up);
        */
    }

    public void DestroyHoverLine()
    {
        Destroy(currentHoverLine);
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
