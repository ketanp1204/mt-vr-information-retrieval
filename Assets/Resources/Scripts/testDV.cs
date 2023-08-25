using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testDV : MonoBehaviour
{
    public DVManager DVManager;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartDV());
    }

    private IEnumerator StartDV()
    {
        yield return new WaitForSeconds(7f);

        DVManager.CreateDVA("GS_B02");
    }
}
