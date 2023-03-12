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
using Photon.Pun;


public class HandRayController : MonoBehaviourPunCallbacks, IPunObservable
{

    public enum Hand
    {
        Left,
        Right
    }

    public Hand hand;
    public InputActionProperty hoverAction;
    public InputActionProperty clickAction;

    public bool hasTooltips = false;
    public TooltipHandler tooltipHandler;
    public Tooltip showRayTooltip;
    public Tooltip pressUITooltip;

    public GameObject hitVisualization;
    public GameObject currentHitGameObject;
    public GameObject xrRayInteractorGO;

    public Vector3 currentHitPoint;

    private GameObject rayVisualization;
    private LineRenderer rayRenderer;
    private UnityEngine.XR.Interaction.Toolkit.XRInteractorLineVisual xrLineVisual;
    private UnityEngine.XR.Interaction.Toolkit.XRRayInteractor xrRayInteractor;
    private UnityEngine.XR.Interaction.Toolkit.ActionBasedController xrController;
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

            Vrsys.ViewingSetupHMDAnatomy viewingSetup = (Vrsys.ViewingSetupHMDAnatomy)Vrsys.NetworkUser.localNetworkUser.viewingSetupAnatomy;
            if (hand == Hand.Left)
            {
                xrRayInteractorGO = Vrsys.Utility.FindRecursive(this.gameObject, "LeftHand Controller");
                xrController = xrRayInteractorGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                Debug.Log(" Selected Left Hand GO: " + xrRayInteractorGO);
            }
            else if (hand == Hand.Right)
            {
                xrRayInteractorGO = Vrsys.Utility.FindRecursive(this.gameObject, "RightHand Controller");
                xrController = xrRayInteractorGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                Debug.Log(" Selected Right Hand GO: " + xrRayInteractorGO);
            }
            xrLineVisual = xrRayInteractorGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractorLineVisual>();
            //xrLineVisual.enabled = isActive;
            xrRayInteractor = xrRayInteractorGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRRayInteractor>();
            rayRenderer = xrRayInteractorGO.GetComponent<LineRenderer>();
            rayRenderer.startWidth = 0.01f;
            rayRenderer.positionCount = 2;
            rayRenderer.enabled = isActive;
            xrRayInteractor.enabled = isActive;
        } else if (!isLocal)
        {
            hitVisualization = CreateDefaultHitVisualization();
            hitVisualization.name = name + " PointingRay Hit Visualization ['" + photonView.Owner.NickName + "']";
            rayVisualization = CreateDefaultRayVisualization();
            rayRenderer = rayVisualization.AddComponent<LineRenderer>();
            rayRenderer.startWidth = 0.01f;
            rayRenderer.positionCount = 2;
            hitVisualization.SetActive(false);
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

            InitializeTooltips();
        }
    }

    void InitializeTooltips()
    {
        if (hasTooltips)
        {
            TooltipHandler tooltipHandler = GetComponent<TooltipHandler>();
            tooltipHandler.ShowTooltip(showRayTooltip, add: true);
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

            bool hoverValue = hoverAction.action.IsPressed();
            if (hoverValue && hoverValue != isActive)
            {
                isActive = true;
                rayRenderer.enabled = true;
                xrLineVisual.enabled = true;
                xrRayInteractor.enabled = true;
                xrRayInteractor.maxRaycastDistance = 10;
                HandlePointing();
                tooltipHandler.HideTooltip(showRayTooltip);
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
                xrRayInteractor.enabled = false;
                xrRayInteractor.maxRaycastDistance = 0;
                hitVisualization.SetActive(false);
                tooltipHandler.ShowTooltip(showRayTooltip, add: true);
            }
        } else if (isInitialized && !isLocal)
        {
            if (!rayRenderer.enabled && isActive)
            {
                rayRenderer.enabled = true;
            } else if (rayRenderer.enabled && !isActive)
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

                tooltipHandler.HideTooltip(pressUITooltip);

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
                tooltipHandler.ShowTooltip(pressUITooltip);

                hitPosition = hitPos;
                //Debug.Log("UI ");
                break;
            case 2: // geometry hit            
                currentHitGameObject = geometryRaycastResult.transform.gameObject;
                currentHitPoint = geometryRaycastResult.point;
                rayRenderer.material = hitSomethingColor;

                rayRenderer.positionCount = 2;
                rayRenderer.SetPosition(0, originPosition);
                rayRenderer.SetPosition(1, geometryRaycastResult.point);

                hitVisualization.transform.position = geometryRaycastResult.point;
                hitVisualization.SetActive(true);

                tooltipHandler.HideTooltip(pressUITooltip);

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
        if (hand == Hand.Left)
        {
            rayVizGO.name = name + "Left Hand Ray Visualization";
        }
        else if (hand == Hand.Right)
        {
            rayVizGO.name = name + "Right Hand Ray Visualization";
        }
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
