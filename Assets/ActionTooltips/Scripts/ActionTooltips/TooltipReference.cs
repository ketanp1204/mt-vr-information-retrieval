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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipReference : MonoBehaviour
{
    public TooltipHand hand;
    public Tooltip.ActionButton reference;
    public TMP_Text tooltipText;
    GameObject userCamera;
    public GameObject connection;
    public GameObject textCanvas;
    public bool isSet = false;
    public bool staticText = false;


    // Start is called before the first frame update
    void Start()
    {
        if (userCamera == null)
        {
            FindCamera();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!staticText) // dynamic text
        {
            if (userCamera == null)
            {
                FindCamera();
            }

            if (textCanvas == null)
            {
                Debug.LogWarning("TooltipReference: Tooltip Text Canvas is not set");
            }

            //// get user camera
            //userCamera = Vrsys.Utility.FindRecursiveInScene("CenterEyeAnchor");

            // rotate text towards user
            if (transform.position - userCamera.transform.position != Vector3.zero)
            {
                textCanvas.transform.rotation = Quaternion.LookRotation(transform.position - userCamera.transform.position);
            }
        }
    }

    public void SetTooltip(Tooltip tooltip)
    {
        tooltipText.text = tooltip.tooltipText;
        isSet = true;
    }

    public void FindCamera()
    {
        if (Vrsys.NetworkUser.localNetworkUser != null)
        {
            userCamera = Vrsys.NetworkUser.localNetworkUser.viewingSetupAnatomy.mainCamera;
        } else
        {
            userCamera = Camera.main.gameObject;
        }
    }
}
