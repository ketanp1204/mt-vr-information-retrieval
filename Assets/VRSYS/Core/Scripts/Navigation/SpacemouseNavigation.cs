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
//   Authors:        Ephraim Schott, Sebastian Muehlhaus, Lucky Chandrautama
//   Date:           2022
//-----------------------------------------------------------------
//
// NOTE:
// Spacemouse requires API Compactibility Level of NET4.xx to function properly.

using UnityEngine;
using UnityEditor;
using SpaceNavigatorDriver;
using Photon.Pun;


namespace Vrsys
{

    public class SpacemouseNavigation : MonoBehaviourPunCallbacks
    {
        public bool isProjectionWallMaster
        {
            get
            {
                var projectionWallConfig = GetComponent<ProjectionWallSystemConfigParser>();
                if (projectionWallConfig == null)
                    return false;
                return projectionWallConfig.localUserSettings.masterFlag;
            }
        }

        [Header("Navigation Platform")]
        [Tooltip("Apply the transfromation by the Spacemouse to the navigation platform target")]
        public bool applyToPlatform = false;
        [Tooltip("The navigation platform target according to PlatformID set in the Navigation Platform Link")]
        public GameObject navigationTarget;


        [Header("Transformation Velocity")]
        [Tooltip("Translation Velocity [m/sec]")]
        [Range(0.1f, 30.0f)]
        public float translationVelocity = 15.0f;

        [Tooltip("Rotation Velocity [degree/sec]")]
        [Range(1.0f, 45.0f)]
        public float rotationVelocity = 20.0f;

        [Header("Rotation Settings")]
        public bool xRotationEnabled = false;
        public bool yRotationEnabled = true;
        public bool zRotationEnabled = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {

#if UNITY_EDITOR

            if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_4_6)
            {
                Debug.LogWarning("Build Target: " + BuildTargetGroup.Standalone.ToString() + ". Spacemouse requires API Compactibility Level of NET4.xx to function properly.");
            }
#endif
            // This script should only compute for the local user
            if (!photonView.IsMine)
                Destroy(this);

            var projectionWallConfig = GetComponent<ProjectionWallSystemConfigParser>();
            if (projectionWallConfig != null && !projectionWallConfig.localUserSettings.masterFlag)
                Destroy(this);

        }



        // Update is called once per frame
        void Update()
        {
            if (EnsureViewingSetup())
            {

                Vector3 transInputVec = SpaceNavigator.Translation * translationVelocity * Time.deltaTime;
                if (transInputVec.magnitude > 0.0f)
                {
                    if (Mathf.Abs((SpaceNavigator.Translation * Time.deltaTime).y) <= 0.00025f)
                    {
                        transInputVec.y = 0.0f;
                    }
                    navigationTarget.transform.Translate(transInputVec);
                }


                Vector3 rotInputVec = CalcRotationInput();
                if (rotInputVec.magnitude > 0.0f)
                {
                    if (xRotationEnabled)
                    {
                        navigationTarget.transform.Rotate(Vector3.right, rotInputVec.x, Space.Self);
                    }
                    if (yRotationEnabled)
                    {
                        navigationTarget.transform.Rotate(Vector3.up, rotInputVec.y, Space.World);
                    }
                    if (zRotationEnabled)
                    {
                        navigationTarget.transform.Rotate(Vector3.forward, rotInputVec.z, Space.Self);
                    }


                }
            }
        }

        private Vector3 CalcRotationInput()
        {
            Vector3 rotation = new Vector3();
            float yaw = SpaceNavigator.Rotation.Yaw() * Mathf.Rad2Deg;
            float pitch = SpaceNavigator.Rotation.Pitch() * Mathf.Rad2Deg;
            float roll = SpaceNavigator.Rotation.Roll() * Mathf.Rad2Deg;
            float inputY = yaw;
            float inputX = pitch;
            float inputZ = roll;

            rotation.x += inputX * rotationVelocity * Time.deltaTime;
            rotation.y += inputY * rotationVelocity * Time.deltaTime;
            rotation.z += inputZ * rotationVelocity * Time.deltaTime;
            return rotation;
        }

        private bool EnsureViewingSetup()
        {
            if (navigationTarget == null)
            {
                if (applyToPlatform)
                {
                    var platformLink = GetComponent<NavigationPlatformLink>();
                    if (platformLink == null || platformLink.platform == null)
                        return false;
                    navigationTarget = platformLink.platform.gameObject;
                    var pView = navigationTarget.GetComponent<PhotonView>();
                    if (pView != null && isProjectionWallMaster)
                        pView.RequestOwnership();
                }
                else if (NetworkUser.localNetworkUser != null && NetworkUser.localNetworkUser.viewingSetupAnatomy != null)
                {
                    navigationTarget = NetworkUser.localNetworkUser.viewingSetupAnatomy.mainCamera;
                }
            }
            return navigationTarget != null;
        }
     
        }

    }
