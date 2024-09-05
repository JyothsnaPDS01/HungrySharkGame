using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.LogError("PLayer is hit");
            Destroy(this.gameObject);
        }
    }
}
