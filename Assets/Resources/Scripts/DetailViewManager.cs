using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DetailViewManager : MonoBehaviourPunCallbacks
{
    /* Public Variables */
    public GameObject detailViewingAreaPrefab;
    public List<int> interestGroupNumbersInUse;

    /* Private Variables */
    private List<Transform> detailViewingAreaTransforms;
    private List<GameObject> usersInDetailView;


    // Start is called before the first frame update
    void Start()
    {
        detailViewingAreaTransforms = new List<Transform>();
        usersInDetailView = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
