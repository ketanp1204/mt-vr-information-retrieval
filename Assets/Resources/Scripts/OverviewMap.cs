using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverviewMap : MonoBehaviour
{
    public GameObject mapCamera;

    public InputActionProperty mapOpenCloseAction;
    public InputActionProperty mapUpAction;
    public InputActionProperty mapDownAction;
    public InputActionProperty mapLeftAction;
    public InputActionProperty mapRightAction;
    public InputActionProperty mapTeleportAction;
    public Animator mapAnimator;

    private bool isMapOpen = false;

    private float mapMoveSpeed = 2f;
    private float moveUp;
    private float moveDown;
    private float moveLeft;
    private float moveRight;

    // Start is called before the first frame update
    void Start()
    {
        mapOpenCloseAction.action.performed += MapOpenClose;
        mapTeleportAction.action.performed += MapTeleport;
    }

    // Update is called once per frame
    void Update()
    {
        moveUp = mapUpAction.action.ReadValue<float>();
        if (moveUp != 0f)
        {
            //Debug.Log(moveUp);
            mapCamera.transform.Translate(Vector3.up * moveUp * mapMoveSpeed * Time.deltaTime);
        }

        moveDown = mapDownAction.action.ReadValue<float>();
        if (moveDown != 0f)
        {
            //Debug.Log(moveDown);
            mapCamera.transform.Translate(Vector3.down * moveDown * mapMoveSpeed * Time.deltaTime);
        }

        moveLeft = mapLeftAction.action.ReadValue<float>();
        if (moveLeft!= 0f)
        {
            //Debug.Log(moveLeft);
            mapCamera.transform.Translate(Vector3.left * moveLeft * mapMoveSpeed * Time.deltaTime);
        }

        moveRight = mapRightAction.action.ReadValue<float>();
        if (moveRight!= 0f)
        {
            //Debug.Log(moveRight);
            mapCamera.transform.Translate(Vector3.right * moveRight * mapMoveSpeed * Time.deltaTime);  
        }
    }

    private void MapOpenClose(InputAction.CallbackContext context)
    {
        Debug.Log("map open");
        if (!isMapOpen)
        {
            mapAnimator.enabled = true;
            mapAnimator.SetTrigger("MapInteract");
        }
        else
        {
            mapAnimator.SetTrigger("MapInteract");
        }
    }

    private void MapTeleport(InputAction.CallbackContext context)
    {
        Vrsys.NetworkUser.localNetworkUser.gameObject.transform.position = mapCamera.transform.position - new Vector3(0, 6f, 0);
    }

}
