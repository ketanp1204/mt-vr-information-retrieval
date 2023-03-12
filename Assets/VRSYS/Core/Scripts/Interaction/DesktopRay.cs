// VRSYS plugin of Virtual Reality and Visualization Research Group (Bauhaus University Weimar)
//  _    ______  _______  _______
// | |  / / __ \/ ___/\ \/ / ___/
// | | / / /_/ /\__ \  \  /\__ \ 
// | |/ / _, _/___/ /  / /___/ / 
// |___/_/ |_|/____/  /_//____/  
//
//  __                            __                       __   __   __    ___ .  . ___
// |__)  /\  |  | |__|  /\  |  | /__`    |  | |\ | | \  / |__  |__) /__` |  |   /\   |  
// |__) /~~\ \__/ |  | /~~\ \__/ .__/    \__/ | \| |  \/  |___ |  \ .__/ |  |  /~~\  |  
//
//       ___               __                                                           
// |  | |__  |  |\/|  /\  |__)                                                          
// |/\| |___ |  |  | /~~\ |  \                                                                                                                                                                                     
//
// Copyright (c) 2022 Virtual Reality and Visualization Research Group
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//-----------------------------------------------------------------
//   Authors:        Ephraim Schott
//   Date:           2022
//-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;


public class DesktopRay : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool alwaysShowRay = true;
    public InputActionProperty showRayAction;

    public GameObject hitVisualization;
    public GameObject currentHitGameObject;
    public GameObject xrRayInteractorGO;
    public GameObject rayController;

    public Vector3 currentHitPoint;

    private GameObject rayVisualization;
    private GameObject desktopCamera;
    private LineRenderer rayRenderer;
    private XRInteractorLineVisual xrLineVisual;
    private XRRayInteractor xrRayInteractor;
    private ActionBasedController xrController;
    public float rayLength = 10.0f;

    // variables for intersection with UI and geometry 
    private bool uiHitFlag;
    private bool geometryHitFlag;
    private UnityEngine.EventSystems.RaycastResult uiRaycastResult;
    private RaycastHit geometryRaycastResult;
    private int intersectionState = 0; // 0 = nothing; 1 = UI; 2 = geometry 

    //colors
    public Material hitNothingColor;
    public Material hitSomethingColor;

    private Vector3 originPosition;
    private Vector3 hitPosition;

    public bool isActive { get; private set; } = false;
    private bool isInitialized = false;
    private bool isLocal = false;


    private void Awake()
    {
        Initialize();
        if (isInitialized && isLocal)
        {
            if (hitVisualization == null)
            {
                hitVisualization = CreateDefaultHitVisualization();
            }
            else
            {
                hitVisualization = Instantiate(hitVisualization);
            }
            rayVisualization = CreateDefaultRayVisualization();

            hitVisualization.name = name + " PointingRay Hit Visualization";
            hitVisualization.SetActive(false);
            desktopCamera = Vrsys.NetworkUser.localNetworkUser.viewingSetupAnatomy.mainCamera;
            rayController = Vrsys.Utility.FindRecursive(gameObject, "RayController");
            
            xrRayInteractorGO = Vrsys.Utility.FindRecursive(rayController, "Ray Interactor");
            xrController = xrRayInteractorGO.GetComponent<ActionBasedController>();

            xrLineVisual = xrRayInteractorGO.GetComponent<XRInteractorLineVisual>();
            //xrLineVisual.enabled = isActive;
            xrRayInteractor = xrRayInteractorGO.GetComponent<XRRayInteractor>();
            rayRenderer = xrRayInteractorGO.GetComponent<LineRenderer>();
            rayRenderer.startWidth = 0.01f;
            rayRenderer.positionCount = 2;
            rayRenderer.enabled = isActive;
        }
        else if (!isLocal)
        {
            hitVisualization = CreateDefaultHitVisualization();
            hitVisualization.name = name + " PointingRay Hit Visualization ['" + photonView.Owner.NickName + "']";
            rayVisualization = CreateDefaultRayVisualization();
            rayRenderer = rayVisualization.AddComponent<LineRenderer>();
            rayRenderer.startWidth = 0.01f;
            rayRenderer.positionCount = 2;
            hitVisualization.SetActive(false);
        }
        if (Camera.main == null)
        {
            Debug.LogError("NO MAIN CAMERA - RAY SELECTION MIGHT BE BUGGY");
        }
    }

    private void Start()
    {
        if (isLocal)
        {
            xrRayInteractorGO.SetActive(true);
            xrLineVisual.enabled = false;
        }
    }


    void Initialize()
    {
        if (isActiveAndEnabled && !isInitialized)
        {
            if (photonView.IsMine)  // init if photon view is mine
            {

                isInitialized = true;
                isLocal = true;
            }
            else
            {
                isInitialized = true;
                isLocal = false;
            }
        }
    }


    private void OnDestroy()
    {
        if (isInitialized)
        {
            if (hitVisualization != null)
            {
                Destroy(hitVisualization);
            }
            if (rayVisualization != null)
            {
                Destroy(rayVisualization);
            }
        }

    }

    private void OnDisable()
    {
        if (isInitialized)
        {
            if (hitVisualization != null)
            {
                hitVisualization.SetActive(false);
            }
            if (rayVisualization != null)
            {
                rayVisualization.SetActive(false);
            }

        }
    }

    private void OnEnable()
    {
        if (rayVisualization == null)
        {
            rayVisualization = CreateDefaultRayVisualization();
        }
        rayVisualization.SetActive(true);
    }

    void Update()
    {
        if (isInitialized && isLocal)
        {
            // clear
            uiHitFlag = false;
            geometryHitFlag = false;
            intersectionState = 0;
            currentHitGameObject = null;
            currentHitPoint = Vector3.zero;

            bool hoverValue = showRayAction.action.IsPressed();
            if (alwaysShowRay)
            {
                hoverValue = true;
            }
            if (hoverValue && hoverValue != isActive)
            {
                isActive = true;
                rayRenderer.enabled = true;
                xrLineVisual.enabled = true;
                xrRayInteractor.maxRaycastDistance = 10;
                HandlePointing();
            }
            else if (hoverValue && isActive)
            {
                HandlePointing();
            }
            else if (!hoverValue && hoverValue != isActive)
            {
                isActive = false;
                rayRenderer.enabled = false;
                xrLineVisual.enabled = false;
                xrRayInteractor.maxRaycastDistance = 0;
                hitVisualization.SetActive(false);
            }
        }
        else if (isInitialized && !isLocal)
        {
            if (!rayRenderer.enabled && isActive)
            {
                rayRenderer.enabled = true;
            }
            else if (rayRenderer.enabled && !isActive)
            {
                rayRenderer.enabled = false;
            }
            if (isActive)
            {
                rayRenderer.positionCount = 2;
                rayRenderer.SetPosition(0, originPosition);
                rayRenderer.SetPosition(1, hitPosition);
            }
        }
    }

    void HandlePointing()
    {
        rayController.transform.position = desktopCamera.transform.position;
        rayController.transform.rotation = Quaternion.LookRotation(desktopCamera.transform.forward, Vector3.up);

        uiHitFlag = xrRayInteractor.TryGetCurrentUIRaycastResult(out uiRaycastResult);
        geometryHitFlag = xrRayInteractor.TryGetCurrent3DRaycastHit(out geometryRaycastResult);
        // identify intersection state
        if (uiHitFlag && !geometryHitFlag)
        {
            intersectionState = 1;
        }
        else if (!uiHitFlag && geometryHitFlag)
        {
            intersectionState = 2;
        }
        else if (uiHitFlag && geometryHitFlag) // both hit -> take nearest intersection
        {
            if (uiRaycastResult.distance <= geometryRaycastResult.distance)
            {
                intersectionState = 1;
            }
            else
            {
                intersectionState = 2;
            }
        }
        else // nothing hit
        {
            intersectionState = 0;
        }

        originPosition = xrRayInteractor.rayOriginTransform.transform.position;
        switch (intersectionState)
        {
            case 0: // nothing hit
                currentHitPoint = new Vector3(0, 0, 0);
                rayRenderer.material = hitNothingColor;

                Vector3 endPoint = originPosition + xrRayInteractorGO.transform.forward * rayLength;
                rayRenderer.positionCount = 2;
                rayRenderer.SetPosition(0, originPosition);
                rayRenderer.SetPosition(1, endPoint);
                hitVisualization.SetActive(false);

                hitPosition = endPoint;

                break;
            case 1: // UI hit
                Vector3 hitPos = uiRaycastResult.worldPosition;
                rayRenderer.material = hitSomethingColor;

                rayRenderer.positionCount = 2;
                rayRenderer.SetPosition(0, originPosition);
                rayRenderer.SetPosition(1, hitPos);

                hitVisualization.transform.position = hitPos;
                hitVisualization.SetActive(true);

                hitPosition = hitPos;
                //Debug.Log("UI ");
                break;
            case 2: // geometry hit            
                currentHitGameObject = geometryRaycastResult.transform.gameObject;
                //Debug.Log(currentHitGameObject.name);
                currentHitPoint = geometryRaycastResult.point;
                rayRenderer.material = hitSomethingColor;

                rayRenderer.positionCount = 2;
                rayRenderer.SetPosition(0, originPosition);
                rayRenderer.SetPosition(1, geometryRaycastResult.point);

                hitVisualization.transform.position = geometryRaycastResult.point;
                hitVisualization.SetActive(true);

                hitPosition = geometryRaycastResult.point;
                break;
        }
    }

    public static GameObject CreateDefaultHitVisualization()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.layer = LayerMask.NameToLayer("Ignore Raycast");
        go.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        go.GetComponent<MeshRenderer>().material.color = Color.red;
        go.GetComponent<SphereCollider>().enabled = false;
        return go;
    }

    public GameObject CreateDefaultRayVisualization()
    {
        var rayVizGO = new GameObject("Ray Visualization");
        rayVizGO.name = name + "Desktop Ray Visualization";

        rayVizGO.layer = LayerMask.NameToLayer("Ignore Raycast");
        return rayVizGO;
    }


    public static int LayermaskToLayer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(isActive);
            stream.SendNext(originPosition);
            stream.SendNext(hitPosition);
        }
        else if (stream.IsReading)
        {
            isActive = (bool)stream.ReceiveNext();
            originPosition = (Vector3)stream.ReceiveNext();
            hitPosition = (Vector3)stream.ReceiveNext();
        }
    }
}



