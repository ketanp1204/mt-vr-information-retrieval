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
//   Authors:        Sebastian Muehlhaus, Lucky Chandrautama
//   Date:           2022
//-----------------------------------------------------------------

using Photon.Pun;
using UnityEngine;

namespace Vrsys 
{
    /*
     * NetworkComponent to place a local onto a NetworkNavigationPlatform.
     * User will inherit the platform transformation and navigation update will be
     * grouped with other users on the platform.
     */
    public class NavigationPlatformLink : MonoBehaviourPunCallbacks, NetworkNavigationPlatformManager.CallbackInterface
    {
        public string platformId = "default";

        [Tooltip("Navigation platform set at runtime.")]
        public NavigationPlatform platform;

        [Tooltip("If a platform of given platformId cannot be found, the PlatformManager will create one at runtime. This property lets you define a custom navigation platform prefab path, which will be instatiated over the network.")]
        public string platformPrefabPath = "";

        public bool requestOwnershipOnStart = true;

        private bool hasRequestedPlatform = false;

        private void Awake()
        {
            if(!hasRequestedPlatform)
                RequestPlatform();
        }

        private void Update()
        {

            if(!hasRequestedPlatform)
                RequestPlatform();
        }

        private void RequestPlatform() 
        {
            if (NetworkNavigationPlatformManager.instance == null) 
            {
                Debug.LogError("your scene needs a NetworkNavigationPlatformManager to use this component");
            }
            NetworkNavigationPlatformManager.instance.RequestPlatform(platformId, this, platformPrefabPath);
            hasRequestedPlatform = true;
        }

        public void OnPlatformReply(string name, GameObject platformGameObject)
        {
            if (name == platformId)
            {
                platform = platformGameObject.GetComponent<NavigationPlatform>();
                transform.SetParent(platform.transform, false);
                if (photonView.IsMine && requestOwnershipOnStart)
                    platform.photonView.RequestOwnership();
            }
        }
    }

}